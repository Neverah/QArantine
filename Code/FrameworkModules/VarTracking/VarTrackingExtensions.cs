namespace QArantine.Code.FrameworkModules.VarTracking
{
    public static class VarTrackingExtensions
    {
        public static void StartVarTracking(string varID, Func<object> updateFunc)
        {
#if !DISABLE_QARANTINE
            VarTracker.Instance.AddTrackedVar(varID, updateFunc);
#endif
        }

        public static void StopVarTracking(string varID)
        {
#if !DISABLE_QARANTINE
            VarTracker.Instance.RemoveTrackedVar(varID);
#endif
        }
    }
}
