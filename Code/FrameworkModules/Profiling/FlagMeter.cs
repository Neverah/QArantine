using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

using QArantine.Code.FrameworkModules;

namespace QArantine.Code.FrameworkModules.Profiling
{
    public class FlagMeter
    {
        public string ID { get; private set; }
        public string Category { get; private set; }
        private List<FlagMeasurement> measurementsBuffer1;
        private List<FlagMeasurement> measurementsBuffer2;
        private List<FlagMeasurement> currentWriteBuffer;
        private List<FlagMeasurement> currentReadBuffer;
        private object _lock = new object();
        private string currCallID = "";
        private long currCallIndex = 0;
        private long currInitTimestamp = 0;
        private int processID;
        private int threadID;
        private bool isMeasurementOngoing = false;

        public FlagMeter (string id, string category, int processID = 1, int threadID = 1)
        {
            ID = id;
            Category = category;
            this.processID = processID;
            this.threadID = threadID;
            measurementsBuffer1 = new List<FlagMeasurement>();
            measurementsBuffer2 = new List<FlagMeasurement>();
            currentWriteBuffer = measurementsBuffer1;
            currentReadBuffer = measurementsBuffer2;
        }

        public void StartMeasurement(long? callIndex = null, string? callID = null)
        {
            lock (_lock)
            {
                if (isMeasurementOngoing)
                {
                    LogError($"Error on measurement for flag {Category}_{ID}_{threadID}: trying to start a measurement for the same flag, category & thread before stopping the previous one");
                    StopMeasurement();
                }
                isMeasurementOngoing = true;
            }

            currCallIndex = callIndex ?? currCallIndex + 1;
            currInitTimestamp = TimeManager.AppClock.ElapsedTicks * 1000000 / Stopwatch.Frequency;
            currCallID = callID ?? "";
        }

        public void StopMeasurement()
        {
            long elapsedMicroseconds = (TimeManager.AppClock.ElapsedTicks * 1000000 / Stopwatch.Frequency) - currInitTimestamp;

            lock (_lock)
            {
                currentWriteBuffer.Add(new FlagMeasurement(currCallIndex, currInitTimestamp, elapsedMicroseconds, currCallID));
                isMeasurementOngoing = false;
            }
        }

        public FlagStats GetFlagStats()
        {
            List<FlagMeasurement> swapBuffer;
            int count = 0;
            lock (_lock)
            {
                // Swap the buffers
                swapBuffer = currentReadBuffer;
                currentReadBuffer = currentWriteBuffer;
                currentWriteBuffer = swapBuffer;

                currentWriteBuffer.Clear();

                count = currentReadBuffer.Count;
            }
            if (count == 0) return new FlagStats(GetFlagStatsID(), 0f, 0f, 0f, 0f, 0);

            double average = Math.Truncate(currentReadBuffer.Average(m => m.ElapsedMicroseconds) * 100.0) / 100.0;
            double max = Math.Truncate(currentReadBuffer.Max(m => m.ElapsedMicroseconds) * 100.0) / 100.0;
            double min = Math.Truncate(currentReadBuffer.Min(m => m.ElapsedMicroseconds) * 100.0) / 100.0;
            double sum = Math.Truncate(currentReadBuffer.Sum(m => m.ElapsedMicroseconds) * 100.0) / 100.0;

            return new FlagStats(GetFlagStatsID(), average, max, min, sum, count);
        }

        public List<object> ExportTraces()
        {
            var traces = new List<object>();
            foreach (var measurement in currentReadBuffer)
            {
                var traceEvent = new
                {
                    name = ID,
                    cat = Category,
                    ph = "X",
                    ts = measurement.Timestamp,
                    dur = measurement.ElapsedMicroseconds,
                    pid = processID,
                    tid = threadID,
                    args = new
                    {
                        call_index = measurement.CallIndex,
                        call_id = measurement.CallID
                    }
                };

                traces.Add(traceEvent);
            }
            return traces;
        }

        private string GetFlagStatsID()
        {
            return $"{Category}_{ID}_{threadID}";
        }
    }
}