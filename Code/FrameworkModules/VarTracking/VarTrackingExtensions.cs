namespace QArantine.Code.FrameworkModules.VarTracking
{
    public static class VarTrackingExtensions
    {
        public static void StartVarTracking(string varID, Func<object> updateFunc)
        {
             VarTracker.Instance.AddTrackedVar(varID, updateFunc);
        }

        public static void StopVarTracking(string varID)
        {
            VarTracker.Instance.RemoveTrackedVar(varID);
        }
    }
}
