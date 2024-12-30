using System.Reflection;
using System.Diagnostics;

using QArantine.Code.FrameworkModules.GUI;
using QArantine.Code.FrameworkModules.GUI.Logs;

namespace QArantine.Code.FrameworkModules.Logs
{
    public class GUILogHandler : ILogHandler
    {
        public static readonly Dictionary<LogManager.LogLevel, string> LogColorConsoleWindowMap = new()
        {
            { LogManager.LogLevel.None, "#99AAB5" },
            { LogManager.LogLevel.FatalError, "#B44141" },
            { LogManager.LogLevel.Error, "#DC4545" },
            { LogManager.LogLevel.OK, "#3CB93C" },
            { LogManager.LogLevel.Warning, "#DCB450" },
            { LogManager.LogLevel.Debug, "#E6E6E6" },
        };

        private bool HasConsoleWindow;

        public GUILogHandler()
        {
            HasConsoleWindow = ConfigManager.GetTFConfigParamAsBool("ConsoleWindowActive") && ConfigManager.GetTFConfigParamAsBool("GUIActive");
        }

        public void LogFatalError(string message)
        {
            LogGeneric(message, LogManager.LogLevel.FatalError);
            if (LogManager.LogLvl >= LogManager.LogLevel.FatalError) PrintFatalErrorCallStack();
        }

        public void LogError(string message)
        {
            LogGeneric(message, LogManager.LogLevel.Error);
            if (LogManager.LogLvl >= LogManager.LogLevel.Debug) PrintCallStack();
        }

        public void LogOK(string message)
        {
            LogGeneric(message, LogManager.LogLevel.OK);
        }

        public void LogWarning(string message)
        {
            LogGeneric(message, LogManager.LogLevel.Warning);
        }

        public void LogDebug(string message)
        {
            LogGeneric(message, LogManager.LogLevel.Debug);
        }

        public void LogTestFatalError(string message)
        {
            LogGeneric(message, LogManager.LogLevel.FatalError, true);
            if (LogManager.LogLvl >= LogManager.LogLevel.FatalError) PrintFatalErrorCallStack();
        }

        public void LogTestError(string message)
        {
            LogGeneric(message, LogManager.LogLevel.Error, true);
            if (LogManager.LogLvl >= LogManager.LogLevel.Debug) PrintCallStack();
        }

        public void LogTestOK(string message)
        {
            LogGeneric(message, LogManager.LogLevel.OK, true);
        }

        public void LogTestWarning(string message)
        {
            LogGeneric(message, LogManager.LogLevel.Warning, true);
        }

        public void LogTestDebug(string message)
        {
            LogGeneric(message, LogManager.LogLevel.Debug, true);
        }

        public bool ThisExecutionHasConsoleWindow()
        {
            return HasConsoleWindow;
        }

        private void LogGeneric(string message, LogManager.LogLevel logLvl, bool isTestLog = false)
        {
            if (LogManager.LogLvl >= logLvl)
            {
                string[] lines = message.Split('\n');
                foreach (string line in lines)
                {
                    WriteLog(line, logLvl, isTestLog);
                }
            }
        }

        private void PrintCallStack()
        {
            StackTrace stackTrace = new();

            Console.ForegroundColor = ConsoleColor.White;
            WriteLog("Call Stack: {", LogManager.LogLevel.Debug);
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                StackFrame? frame = stackTrace.GetFrame(i);
                MethodBase? method = frame?.GetMethod();
                WriteLog($"\t- {method?.DeclaringType}.{method?.Name}", LogManager.LogLevel.Debug);
            }
            WriteLog("}", LogManager.LogLevel.Debug);
        }

        private void PrintFatalErrorCallStack()
        {
            StackTrace stackTrace = new();

            Console.ForegroundColor = ConsoleColor.DarkRed;
            WriteLog("Call Stack: {", LogManager.LogLevel.FatalError);
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                StackFrame? frame = stackTrace.GetFrame(i);
                MethodBase? method = frame?.GetMethod();
                WriteLog($"\t- {method?.DeclaringType}.{method?.Name}", LogManager.LogLevel.FatalError);
            }
            WriteLog("}", LogManager.LogLevel.FatalError);
        }

        private string GetLogTestPrefix()
        {
            if (LogManager.TestManager?.CurrentTest != null && LogManager.TestManager?.CurrentTest?.CurrentTestCase != null)
            {
                return $"<[T]:{LogManager.TestManager?.CurrentTest.CurrentTestCase.ID}:{LogManager.TestManager?.CurrentTest.CurrentTestCase.CurrentStep}> ";
            }
            return "";
        }

        private string GetFormatedElapsedTime()
        {
            return "|" + TimeManager.GetAppElapsedSecondsAsString() + "| ";
        }

        private void WriteLog(string message, LogManager.LogLevel logLvl, bool printPrefixOnFile = false)
        {
            if(HasConsoleWindow)
            {
                WriteLogOnConsoleWindow(message, logLvl, printPrefixOnFile);
            }
        }

        internal string GetFinalLogLineString(string message, string logClassName, bool printPrefix)
        {
            if (printPrefix) return "<p class='" + logClassName + "'><span class='time-tag'>" + GetFormatedElapsedTime() + "</span><span class='test-log-prefix'>" + GetLogTestPrefix() + "</span> " + message + "</p>";
            return "<p class='" + logClassName + "'><span class='time-tag'>" + GetFormatedElapsedTime() + "</span>" + message + "</p>";
        }

        private void WriteLogOnConsoleWindow(string message, LogManager.LogLevel lvl, bool printPrefix)
        {
            GUIManager.Instance.GUILogBuffer.AddLogLine(GetFinalConsoleWindowLogLine(message, lvl, printPrefix));
        }

        internal BufferLogLine GetFinalConsoleWindowLogLine(string message, LogManager.LogLevel lvl, bool printPrefix)
        {
            if (printPrefix) return new BufferLogLine(GetFormatedElapsedTime(), "#A0A0A0", GetLogTestPrefix(), "#A064DC", message, LogColorConsoleWindowMap[lvl]);
            return new BufferLogLine(GetFormatedElapsedTime(), "#A0A0A0", "", "#A064DC", message, LogColorConsoleWindowMap[lvl]);
        }
    }
}