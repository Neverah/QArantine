namespace QArantine.Code.FrameworkModules.Profiling
{
    public class FlagMeasurement
    {
        public long ElapsedMicroseconds { get; private set; }
        public long Timestamp { get; private set; }
        public long CallIndex { get; private set; }
        public string? CallID { get; private set; }

        public FlagMeasurement (long callIndex, long timestamp, long elapsedMicroseconds, string? callID = null)
        {
            CallIndex = callIndex;
            Timestamp = timestamp;
            ElapsedMicroseconds = elapsedMicroseconds;
            CallID = callID ?? "";
        }
    }
}