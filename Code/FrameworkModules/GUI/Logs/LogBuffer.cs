using System.Collections.ObjectModel;

namespace QArantine.Code.FrameworkModules.GUI.Logs
{
    public class LogBuffer
    {
        public readonly ObservableCollection<BufferLogLine> LogLinesBuffer;
        public event EventHandler? LogLinesAdded;
        public readonly Mutex LogMutex;

        public LogBuffer()
        {
            LogMutex = new();
            LogLinesBuffer = [];
        }

        public void AddLogLine(BufferLogLine logLine)
        {
            LogMutex.WaitOne();

            try
            {
                LogLinesBuffer.Add(logLine);
            }
            finally
            {
                LogMutex.ReleaseMutex();
            }
            LogLinesAdded?.Invoke(this, EventArgs.Empty);
        }

        public void AddLogLine(string timestamp, string timestampForegroundHexCode, string testTag, string testTagForegroundHexCode, string logBody, string logBodyForegroundHexCode)
        {
            AddLogLine(new BufferLogLine(timestamp, timestampForegroundHexCode, testTag, testTagForegroundHexCode, logBody, logBodyForegroundHexCode));
        }
    }
}