using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;

using QArantine.Code.FrameworkModules;

namespace QArantine.Code.FrameworkModules.Profiling
{
    public class TFProfiler
    {
        private static TFProfiler? _instance;
        private Process currentProcess;
        private ConcurrentDictionary<string, FlagMeter> flagMeters;
        private Timer? updateTimer;
        private StreamWriter? fileWriter;
        private readonly object _fileLock = new object();
        private readonly object _updateLock = new object();
        private int updateInterval { get; set; } = 1000; // 1 second
        private bool isUpdateTimerActive = false;
        public bool isMeasurementActive { get; private set; } = false;
        public bool isFileDumpActive { get; private set; } = false;
        public string baseDumpFileName { get; set; } = "ProfMeasurements";
        public event EventHandler<ProfilingDataEventArgs>? StatsUpdated;

        public int UpdateInterval
        {
            get => updateInterval;
            set
            {
                updateInterval = value;
                ChangeUpdateTimerInterval();
            }
        }

        private TFProfiler()
        {
            flagMeters = new ConcurrentDictionary<string, FlagMeter>();
            currentProcess = Process.GetCurrentProcess();
            SetMeasurementEnabledState(ConfigManager.GetTFConfigParamAsBool("ProfilerActive"));
        }

        public static TFProfiler Instance
        {
            get
            {
                _instance ??= new();
                return _instance;
            }
        }

        private void StartMeasurement()
        {
            flagMeters = new ConcurrentDictionary<string, FlagMeter>();
        }

        private void StartFileDump()
        {
            lock (_fileLock)
            {
                fileWriter?.Close();
                fileWriter = new StreamWriter(baseDumpFileName + "_" + DateTime.Now.Ticks.ToString() + ".json", false);
                fileWriter.WriteLine("{\"traceEvents\": [");
            }
        }

        private void FinalizeTrace()
        {
            lock (_fileLock)
            {
                fileWriter?.WriteLine("]}");
                fileWriter?.Close();
                fileWriter = null;
            }
        }

        public void StartFlagMeasurement(string flagID, string category, long? callIndex = null, string? callID = null)
        {
            if (!isMeasurementActive) return;

            int threadID = Thread.CurrentThread.ManagedThreadId;
            string key = GetFlagKey(flagID, category, threadID);
            FlagMeter flagMeter = flagMeters.GetOrAdd(key, id => new FlagMeter(flagID, category, processID: 1, threadID));
            flagMeter.StartMeasurement(callIndex, callID);
        }

        public void StopFlagMeasurement(string flagID, string category)
        {
            if (!isMeasurementActive) return;

            int threadID = Thread.CurrentThread.ManagedThreadId;
            string key = GetFlagKey(flagID, category, threadID);
            if (flagMeters.TryGetValue(key, out FlagMeter? flagMeter))
            {
                flagMeter?.StopMeasurement();
            }
        }

        private void UpdateGatheredData(object? state)
        {
            List<FlagStats> newStats = new List<FlagStats>();

            lock (_updateLock)
            {
                foreach (FlagMeter flagMeter in flagMeters.Values)
                {
                    FlagStats stats = flagMeter.GetFlagStats();
                    newStats.Add(stats);

                    if (isFileDumpActive)
                    {
                        List<object> traces = flagMeter.ExportTraces();
                        if (traces.Count > 0)
                        {
                            lock (_fileLock)
                            {
                                foreach (var trace in traces)
                                {
                                    var json = JsonSerializer.Serialize(trace);
                                    fileWriter?.WriteLine(json + ",");
                                }
                            }
                        }
                    }
                }
            }

            StatsUpdated?.Invoke(this, new ProfilingDataEventArgs(newStats, GetMemoryWorkingSetSysStats(), GetPrivateMemorySizeSysStats()));
        }

        private string GetFlagKey(string flagID, string category, int threadID)
        {
            return $"{category}_{flagID}_{threadID}";
        }

        public void SetMeasurementEnabledState(bool enabled)
        {
            if (!isMeasurementActive && enabled)
            {
                StartMeasurement();
                StartUpdateTimer();
            }

            isMeasurementActive = enabled;

            if (!isMeasurementActive && isFileDumpActive) SetFileDumpEnabledState(false);
            if (!isMeasurementActive) StopUpdateTimer();
        }

        public void SetFileDumpEnabledState(bool enabled)
        {   
            if (!isFileDumpActive && enabled && isMeasurementActive) StartFileDump();
            if (isFileDumpActive && !enabled) FinalizeTrace();

            isFileDumpActive = enabled && isMeasurementActive;
        }

        private void StartUpdateTimer()
        {
            lock (_updateLock)
            {
                if (isUpdateTimerActive) return;

                updateTimer = new Timer(UpdateGatheredData, null, updateInterval, updateInterval);
                isUpdateTimerActive = true;
            }
        }

        private void StopUpdateTimer()
        {
            lock (_updateLock)
            {
                if (!isUpdateTimerActive) return;

                updateTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                updateTimer?.Dispose();
                updateTimer = null;
                isUpdateTimerActive = false;
            }
        }

        private void ChangeUpdateTimerInterval()
        {
            lock (_updateLock)
            {
                updateTimer?.Change(updateInterval, updateInterval);
            }
        }

        private SysStats GetMemoryWorkingSetSysStats()
        {
            return new SysStats("Working Set", GetMemoryWorkingSet());
        }

        private SysStats GetPrivateMemorySizeSysStats()
        {
            return new SysStats("Private Memory", GetPrivateMemorySize());
        }

        private double GetMemoryWorkingSet()
        {
            return (double)currentProcess.WorkingSet64;
        }

        private double GetPrivateMemorySize()
        {
            return (double)currentProcess.PrivateMemorySize64;
        }
    }
}