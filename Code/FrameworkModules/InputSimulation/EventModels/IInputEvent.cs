namespace QArantine.Code.FrameworkModules.InputSimulation
{
    public interface IInputEvent
    {
        string DeviceType { get; }
        public long Timestamp { get; set; } // For internal continuos events recording (microseconds)
        public long? FrameId { get; set; } // For external events only
    }
}