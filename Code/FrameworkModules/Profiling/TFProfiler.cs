using System.Diagnostics;
using System.Collections.Concurrent;
using System.Text.Json;

namespace QArantine.Code.FrameworkModules.Profiling
{
    public class TFProfiler
    {
        private static TFProfiler? _instance;
        private Process currentProcess;
        private ConcurrentDictionary<string, FlagMeter> flagMeters;
        private Timer? updateTimer;
        private StreamWriter? fileWriter;
        private readonly object _fileLock = new();
        private readonly object _updateLock = new();
        private int _updateInterval { get; set; } = 1000; // 1 second
        private bool isUpdateTimerActive = false;
        public bool IsMeasurementActive { get; private set; } = false;
        public bool IsFileDumpActive { get; private set; } = false;
        public string BaseDumpFileName { get; set; } = "ProfMeasurements";
        public event EventHandler<ProfilingDataEventArgs>? StatsUpdated;

        public int UpdateInterval
        {
            get => _updateInterval;
            set
            {
                _updateInterval = value;
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
                fileWriter = new StreamWriter(BaseDumpFileName + "_" + DateTime.Now.Ticks.ToString() + ".json", false);
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
            if (!IsMeasurementActive) return;

            int threadID = Thread.CurrentThread.ManagedThreadId;
            string key = GetFlagKey(flagID, category, threadID);
            FlagMeter flagMeter = flagMeters.GetOrAdd(key, id => new FlagMeter(flagID, category, processID: 1, threadID));
            flagMeter.StartMeasurement(callIndex, callID);
        }

        public void StopFlagMeasurement(string flagID, string category)
        {
            if (!IsMeasurementActive) return;

            int threadID = Thread.CurrentThread.ManagedThreadId;
            string key = GetFlagKey(flagID, category, threadID);
            if (flagMeters.TryGetValue(key, out FlagMeter? flagMeter))
            {
                flagMeter?.StopMeasurement();
            }
        }

        private void UpdateGatheredData(object? state)
        {
            List<FlagStats> newStats = [];

            lock (_updateLock)
            {
                foreach (FlagMeter flagMeter in flagMeters.Values)
                {
                    FlagStats stats = flagMeter.GetFlagStats();
                    newStats.Add(stats);

                    if (IsFileDumpActive)
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
            if (!IsMeasurementActive && enabled)
            {
                StartMeasurement();
                StartUpdateTimer();
            }

            IsMeasurementActive = enabled;

            if (!IsMeasurementActive && IsFileDumpActive) SetFileDumpEnabledState(false);
            if (!IsMeasurementActive) StopUpdateTimer();
        }

        public void SetFileDumpEnabledState(bool enabled)
        {   
            if (!IsFileDumpActive && enabled && IsMeasurementActive) StartFileDump();
            if (IsFileDumpActive && !enabled) FinalizeTrace();

            IsFileDumpActive = enabled && IsMeasurementActive;
        }

        private void StartUpdateTimer()
        {
            lock (_updateLock)
            {
                if (isUpdateTimerActive) return;

                updateTimer = new Timer(UpdateGatheredData, null, _updateInterval, _updateInterval);
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
                updateTimer?.Change(_updateInterval, _updateInterval);
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
            return currentProcess.WorkingSet64;
        }

        private double GetPrivateMemorySize()
        {
            return currentProcess.PrivateMemorySize64;
        }
    }
}