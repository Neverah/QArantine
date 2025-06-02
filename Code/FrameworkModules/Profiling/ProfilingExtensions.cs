namespace QArantine.Code.FrameworkModules.Profiling
{
    public static class ProfilingExtensions
    {
        public static void StartProfilerFlagMeasurement(string flagID, string category, long callIndex = -1, string? callID = null)
        {
#if !DISABLE_QARANTINE
            TFProfiler.Instance.StartFlagMeasurement(flagID, category, callIndex, callID);
#endif
        }

        public static void StopProfilerFlagMeasurement(string flagID, string category)
        {
#if !DISABLE_QARANTINE
            TFProfiler.Instance.StopFlagMeasurement(flagID, category);
#endif
        }
    }
}
