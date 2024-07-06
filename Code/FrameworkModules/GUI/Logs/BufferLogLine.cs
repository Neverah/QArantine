namespace QArantine.Code.FrameworkModules.GUI.Logs
{
    public class BufferLogLine
    {
        public string Timestamp { get; set; }
        public string TimestampForegroundHexCode{ get; set; }
        public string TestTag { get; set; }
        public string TestTagForegroundHexCode { get; set; }
        public string LogBody { get; set; }
        public string LogBodyForegroundHexCode { get; set; }

        public BufferLogLine(string timestamp, string timestampForegroundHexCode, string testTag, string testTagForegroundHexCode, string logBody, string logBodyForegroundHexCode)
        {
            Timestamp = timestamp ?? "";
            TimestampForegroundHexCode = timestampForegroundHexCode ?? "#FFFFFF";
            TestTag = testTag ?? "";
            TestTagForegroundHexCode = testTagForegroundHexCode ?? "#FFFFFF";
            LogBody = logBody ?? "";
            LogBodyForegroundHexCode = logBodyForegroundHexCode ?? "#FFFFFF";
        }
    }
}