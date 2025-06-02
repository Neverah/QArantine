namespace QArantine.Code.FrameworkModules.InputSimulation
{
    public class KeyboardEvent : IInputEvent
    {
        public string DeviceType => "Keyboard";
        public long Timestamp { get; set; }
        public long? FrameId { get; set; }
        public required string Key { get; set; }
        public bool IsPressed { get; set; }
    }
}