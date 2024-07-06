namespace QArantine.Code.FrameworkModules.Profiling
{
    public static class ProfilingExtensions
    {
        public static void StartProfilerFlagMeasurement(string flagID, string category, long? callIndex = null, string? callID = null)
        {
            TFProfiler.Instance.StartFlagMeasurement(flagID, category, callIndex, callID);
        }

        public static void StopProfilerFlagMeasurement(string flagID, string category)
        {
            TFProfiler.Instance.StopFlagMeasurement(flagID, category);
        }
    }
}
