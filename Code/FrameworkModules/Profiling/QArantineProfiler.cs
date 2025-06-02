using System;

namespace QArantine.Code.FrameworkModules.Profiling
{
    public static class QArantineProfiler
    {
        public static void WithProfiler(string flagName, string category, Action action, long callIndex = -1, string? callID = null)
        {
            ProfilingExtensions.StartProfilerFlagMeasurement(flagName, category, callIndex, callID);

            try
            {
                action();
            }
            finally
            {
                ProfilingExtensions.StopProfilerFlagMeasurement(flagName, category);
            }
        }
    }
}