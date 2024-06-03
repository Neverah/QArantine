using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using TestFramework.Code.FrameworkModules;
using TestFramework.Code.TestFrameworkGUI;
using TestFramework.Code.TestFrameworkGUI.ViewModels;

namespace TestFramework.Code.FrameworkModules.GUI.Logs
{
    public class LogBuffer
    {
        public readonly ObservableCollection<BufferLogLine> LogLinesBuffer;
        public event EventHandler? LogLinesAdded;
        public readonly Mutex LogMutex;

        public LogBuffer()
        {
            LogMutex = new();
            LogLinesBuffer = new();
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