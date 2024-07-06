namespace QArantine.Code.FrameworkModules.Profiling
{
    public class SysStats
    {
        public string ID { get; private set; }
        public double Value { get; private set; }

        public SysStats (string id, double value)
        {
            ID = id;
            Value = value;
        }
    }
}