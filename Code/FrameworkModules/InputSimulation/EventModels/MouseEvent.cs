namespace QArantine.Code.FrameworkModules.InputSimulation
{
    public class MouseEvent : IInputEvent
    {
        public string DeviceType => "Mouse";
        public long Timestamp { get; set; }
        public long? FrameId { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public string? Button { get; set; }
        public bool IsPressed { get; set; }
    }
}