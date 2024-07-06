namespace QArantine.Code.FrameworkModules.Profiling
{
    public class FlagStats
    {
        public string ID { get; private set; }
        public double Average { get; private set; }
        public double Max { get; private set; }
        public double Min { get; private set; }
        public double Sum  { get; private set; }
        public int Count { get; private set; }

        public FlagStats (string id, double average, double max, double min, double sum, int count)
        {
            ID = id;
            Average = average;
            Max = max;
            Min = min;
            Sum = sum;
            Count = count;
        }
    }
}