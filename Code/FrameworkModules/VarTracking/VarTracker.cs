using System.Collections.Concurrent;

namespace QArantine.Code.FrameworkModules.VarTracking
{
    public class VarTracker
    {
        private static VarTracker? _instance;
        private ConcurrentDictionary<string, Func<object>> varsValues;
        private Timer? updateTimer;
        private readonly object _updateLock = new();
        private int _updateInterval { get; set; } = 100; // 0.1 second
        private bool isUpdateTimerActive = false;
        public bool IsTrackingActive { get; private set; } = false;
        public event EventHandler<VarTrackerUpdateEventArgs>? VarValuesUpdated;

        public int UpdateInterval
        {
            get => _updateInterval;
            set
            {
                _updateInterval = value;
                ChangeUpdateTimerInterval();
            }
        }

        private VarTracker()
        {
            varsValues = [];
            SetTrackingEnabledState(ConfigManager.GetTFConfigParamAsBool("VarTrackerActive"));
        }

        public static VarTracker Instance
        {
            get
            {
                _instance ??= new();
                return _instance;
            }
        }

        public void AddTrackedVar(string varID, Func<object> updateFunc)
        {
            varsValues.AddOrUpdate(varID, updateFunc, (key, oldValue) => updateFunc);
        }

        public void RemoveTrackedVar(string varID)
        {
            varsValues.Remove(varID, out var outValue);
        }

        private void UpdateGatheredData(object? state)
        {
            QArantineProfiler.WithProfiler("VarTracker.UpdateGatheredData()", "QArantine_VarTracker", () =>
            {
                List<TrackedVar> newTrackedVars = [];
                List<string> keysToRemove = [];

                lock (_updateLock)
                {
                    foreach (KeyValuePair<string, Func<object>> kvp in varsValues)
                    {
                        newTrackedVars.Add(new TrackedVar(kvp.Key, kvp.Value));
                    }
                }

                VarValuesUpdated?.Invoke(this, new VarTrackerUpdateEventArgs(newTrackedVars));
            });
        }

        public void SetTrackingEnabledState(bool enabled)
        {
            if (!IsTrackingActive && enabled)
            {
                StartUpdateTimer();
            }

            IsTrackingActive = enabled;

            if (!IsTrackingActive) StopUpdateTimer();
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
    }
}