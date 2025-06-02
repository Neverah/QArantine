using System;

namespace QArantine.Code.FrameworkModules.Profiling
{
    public class QArantineProfilerScope : IDisposable
    {
        private readonly string _flagName;
        private readonly string _category;
        private readonly long _callIndex;
        private readonly string? _callID;

        public QArantineProfilerScope(string flagName, string category, long callIndex = -1, string? callID = null)
        {
            _flagName = flagName;
            _category = category;
            _callIndex = callIndex;
            _callID = callID;

            ProfilingExtensions.StartProfilerFlagMeasurement(_flagName, _category, _callIndex, _callID);
        }

        public void Dispose()
        {
            ProfilingExtensions.StopProfilerFlagMeasurement(_flagName, _category);
        }
    }
}