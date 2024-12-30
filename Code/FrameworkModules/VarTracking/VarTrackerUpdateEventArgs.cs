namespace QArantine.Code.FrameworkModules.VarTracking
{
    public class VarTrackerUpdateEventArgs(List<TrackedVar> trackedVars) : EventArgs
    {
        public List<TrackedVar> TrackedVars { get; private set; } = trackedVars;
    }
}