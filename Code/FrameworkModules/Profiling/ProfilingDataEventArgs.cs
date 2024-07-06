namespace QArantine.Code.FrameworkModules.Profiling
{
    public class ProfilingDataEventArgs : EventArgs
    {
        public List<FlagStats> FlagsStats { get; private set; }
        public SysStats MemoryWorkingSetSysStats { get; private set; }
        public SysStats PrivateMemorySizeSysStats { get; private set; }

        public ProfilingDataEventArgs(List<FlagStats> flagsStats, SysStats memoryWorkingSetSysStats, SysStats privateMemorySizeSysStats)
        {
            FlagsStats = flagsStats;
            MemoryWorkingSetSysStats = memoryWorkingSetSysStats;
            PrivateMemorySizeSysStats = privateMemorySizeSysStats;
        }
    }
}