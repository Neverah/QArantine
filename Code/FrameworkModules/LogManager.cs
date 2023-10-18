using System.Diagnostics;
using System.Reflection;

namespace TestFramework.Code.FrameworkModules
{
    public static class LogManager
    {
        private static string LogPath;
        private static string ErrorsLogPath;
        private static bool LogsOpen = false;
        private static bool DumpToLogFiles = false;
        private static StreamWriter? LogFile;
        private static StreamWriter? ErrorsLogFile;

        public enum LogLevel
        {
            None,
            Error,
            OK,
            Warning,
            Debug
        }

        public static LogLevel LogLvl { get; set; }
        public static TestManager? TestManager { get; set; }

        static LogManager()
        {
            LogLvl = LogLevel.Warning;
            LogPath = "";
            ErrorsLogPath = "";
        }

        public static void LogError(string message)
        {
            if (LogLvl >= LogLevel.Error)
            {
                PrintConsoleLogTimePrefix();
                Console.ForegroundColor = ConsoleColor.Red;
                WriteLog($"{message}", LogLevel.Error);
            }

            if (LogLvl >= LogLevel.Debug) PrintCallStack();
        }

        public static void LogOK(string message)
        {
            if (LogLvl >= LogLevel.OK)
            {
                PrintConsoleLogTimePrefix();
                Console.ForegroundColor = ConsoleColor.Green;
                WriteLog($"{message}", LogLevel.OK);
            }
        }

        public static void LogWarning(string message)
        {
            if (LogLvl >= LogLevel.Warning)
            {
                PrintConsoleLogTimePrefix();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                WriteLog($"{message}", LogLevel.Warning);
            }
        }

        public static void LogDebug(string message)
        {
            if (LogLvl >= LogLevel.Debug)
            {
                PrintConsoleLogTimePrefix();
                Console.ForegroundColor = ConsoleColor.White;
                WriteLog($"{message}", LogLevel.Debug);
            }
        }

        public static void LogTestError(string message)
        {
            if (LogLvl >= LogLevel.Error)
            {
                PrintConsoleLogTimePrefix();
                PrintConsoleLogTestPrefix();
                Console.ForegroundColor = ConsoleColor.Red;
                WriteLog($"{message}", LogLevel.Error, true);
            }

            if (LogLvl >= LogLevel.Debug) PrintCallStack();
        }

        public static void LogTestOK(string message)
        {
            if (LogLvl >= LogLevel.OK)
            {
                PrintConsoleLogTimePrefix();
                PrintConsoleLogTestPrefix();
                Console.ForegroundColor = ConsoleColor.Green;
                WriteLog($"{message}", LogLevel.OK, true);
            }
        }

        public static void LogTestWarning(string message)
        {
            if (LogLvl >= LogLevel.Warning)
            {
                PrintConsoleLogTimePrefix();
                PrintConsoleLogTestPrefix();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                WriteLog($"{message}", LogLevel.Warning, true);
            }
        }

        public static void LogTestDebug(string message)
        {
            if (LogLvl >= LogLevel.Debug)
            {
                PrintConsoleLogTimePrefix();
                PrintConsoleLogTestPrefix();
                Console.ForegroundColor = ConsoleColor.White;
                WriteLog($"{message}", LogLevel.Debug, true);
            }
        }

        public static void StartLogFile()
        {
            if(!LogsOpen && ThisExecutionHasLogFileDump())
            {
                InitLogLevel();
                DeleteOldLogFiles();
                CreateLogFiles();
                LogsOpen = true;
                DumpToLogFiles = true;
            }
        }

        public static string GetLogPath()
        {
            InitLogPath();
            return LogPath;
        }

        public static string GetErrorsLogPath()
        {
            InitErrorsLogPath();
            return ErrorsLogPath;
        }

        public static bool IsLogFileDumpActive()
        {
            return DumpToLogFiles;
        }

        public static bool ThisExecutionHasLogFileDump()
        {
            return ConfigManager.GetTFConfigParam("DumpLogsToFile") == "true";
        }

        public static bool ThisExecutionHasErrorLogFileDump()
        {
            return ThisExecutionHasLogFileDump() && ConfigManager.GetTFConfigParam("ErrorsLogActive") == "true";
        }

        private static void CloseLogFiles()
        {
            DumpToLogFiles = false;
            CloseLogFile();
            CloseErrorsLogFile();
            LogsOpen = false;
        }

        private static void CloseLogFile()
        {
            if (LogsOpen)
            {
                LogFile?.WriteLine("<p class='ok'><span class='time-tag'>" + GetFormatedElapsedTime() + "</span>Closing the log...</p>");
                LogFile?.WriteLine("</body>");
                LogFile?.WriteLine("</html>");
                LogFile?.Close();
            }
        }

        private static void CloseErrorsLogFile()
        {
            if (LogsOpen && ThisExecutionHasErrorLogFileDump())
            {
                ErrorsLogFile?.WriteLine("<p class='ok'><span class='time-tag'>" + GetFormatedElapsedTime() + "</span>Closing the errors log...</p>");
                ErrorsLogFile?.WriteLine("</body>");
                ErrorsLogFile?.WriteLine("</html>");
                ErrorsLogFile?.Close();
            }
        }

        private static void InitLogLevel()
        {
            string logLvlName;
            if ((logLvlName = ConfigManager.GetTFConfigParam("LogLevel")!) == null)
            {
                LogError("Could not find the 'LogLevel' config param, the log can not be opened, aborting execution");
                Environment.Exit(-1);
            }

            LogLvl = (LogLevel)Enum.Parse(typeof(LogLevel), logLvlName);
        }

        private static void InitLogPath()
        {
            if (LogPath != null && LogPath != "") return;

            if ((LogPath = ConfigManager.GetTFConfigParam("LogPath")!) == null)
            {
                LogError("Could not find the 'LogPath' config param. The log can not be opened, aborting execution");
                Environment.Exit(-1);
            }
            
            if (!Path.IsPathRooted(LogPath)) LogPath = Path.Combine(Environment.CurrentDirectory, LogPath);
        }

        private static void InitErrorsLogPath()
        {
            if (ErrorsLogPath != null && ErrorsLogPath != "") return;

            if (ThisExecutionHasErrorLogFileDump())
            {
                if ((ErrorsLogPath = ConfigManager.GetTFConfigParam("ErrorsLogPath")!) == null)
                {
                    LogError("Could not find the 'ErrorsLogPath' config param, but the 'ErrorsLogActive' config param is set to 'true'. The errors log can not be opened, aborting execution");
                    Environment.Exit(-1);
                }
                
                if (!Path.IsPathRooted(ErrorsLogPath)) ErrorsLogPath = Path.Combine(Environment.CurrentDirectory, ErrorsLogPath);
            }
        }

        private static void PrintCallStack()
        {
            StackTrace stackTrace = new();

            Console.ForegroundColor = ConsoleColor.White;
            WriteLog("Call Stack:\n{", LogLevel.Debug);
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                StackFrame? frame = stackTrace.GetFrame(i);
                MethodBase? method = frame.GetMethod();
                WriteLog($"\t- {method?.DeclaringType}.{method?.Name}", LogLevel.Debug);
            }
            WriteLog("}", LogLevel.Debug);
        }

        private static void PrintConsoleLogTimePrefix()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(GetFormatedElapsedTime());
        }

        private static void PrintConsoleLogTestPrefix()
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write(GetLogTestPrefix());
        }

        private static string GetLogTestPrefix(bool isForHTML = false)
        {
            if (TestManager?.CurrentTest != null && TestManager?.CurrentTest?.CurrentTestCase != null)
            {
                if (isForHTML) return $"&lt;{TestManager?.CurrentTest.CurrentTestCase.ID}:{TestManager?.CurrentTest.CurrentTestCase.CurrentStep}&gt; ";
                else return $"<{TestManager?.CurrentTest.CurrentTestCase.ID}:{TestManager?.CurrentTest.CurrentTestCase.CurrentStep}> ";
            }
            return "";
        }

        private static string GetFormatedElapsedTime()
        {
            return "|" + TimeManager.AppClock.Elapsed.TotalSeconds.ToString("0.00") + "| ";
        }

        private static void DeleteOldLogFiles()
        {
            if (File.Exists(GetLogPath())) File.Delete(GetLogPath());
            if (File.Exists(GetErrorsLogPath())) File.Delete(GetErrorsLogPath());
        }

        private static void CreateLogFiles()
        {
            CreateLogFile();
            CreateErrorsLogFile();

            // Event handler for closing the log on program exit
            AppDomain.CurrentDomain.ProcessExit += (sender, args) => CloseLogFiles();
        }

        private static void CreateLogFile()
        {
            CreateGenericLogFile(GetLogPath(), out LogFile);
        }

        private static void CreateErrorsLogFile()
        {
            if (ThisExecutionHasErrorLogFileDump()) CreateGenericLogFile(GetErrorsLogPath(), out ErrorsLogFile);
        }

        private static void CreateGenericLogFile(string logPath, out StreamWriter outLogWriter)
        {
            outLogWriter = new(logPath, append: true);

            string htmlHeader = @"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    /* Paragraph style */
                    p {
                        margin: 5px 0;
                        font-size: 13px;
                        font-weight: bold;
                        font-family: 'Arial', sans-serif;
                    }
                    /* Background style */
                    body { background-color: #111; color: white; }
                    /* Styles for different log tags */
                    .error { color: rgb(220, 69, 69); }
                    .ok { color: rgb(60, 185, 60); }
                    .warning { color: rgb(220, 180, 80) }
                    .debug { color: rgb(230, 230, 230); }
                    .test-log-prefix { color: rgb(160, 100, 220); }
                    .time-tag { color: rgb(160, 160, 160); }
                </style>
            </head>
            <body>";
            outLogWriter.WriteLine(htmlHeader);
        }

        private static void WriteLog(string message, LogLevel lvl, bool printPrefixOnFile = false)
        {
            Console.WriteLine(message);
            if (DumpToLogFiles && LogsOpen) 
            {
                WriteLogOnLogFiles(message, lvl, printPrefixOnFile);
            }
        }

        private static void WriteLogOnLogFiles(string message, LogLevel lvl, bool printPrefix)
        {
            WriteLogOnLogFile(LogFile!, message, lvl, printPrefix);
            if(lvl == LogLevel.Error) WriteLogOnLogFile(ErrorsLogFile!, message, lvl, false);
        }

        private static void WriteLogOnLogFile(StreamWriter logFile, string message, LogLevel lvl, bool printPrefix)
        {
            string logClassName;
            switch (lvl)
            {
                case LogLevel.Error:
                    logClassName = "error";
                    break;

                case LogLevel.OK:
                    logClassName = "ok";
                    break;

                case LogLevel.Warning:
                    logClassName = "warning";
                    break;

                case LogLevel.Debug:
                    logClassName = "debug";
                    break;

                default:
                    logClassName = "error";
                    message = "[ERROR: THIS LOG HAS NO DEFINED TYPE] - " + message;
                    break;
            }

            if (printPrefix) logFile?.WriteLine("<p class='" + logClassName + "'><span class='time-tag'>" + GetFormatedElapsedTime() + "</span><span class='test-log-prefix'>" + GetLogTestPrefix(true) + "</span> " + message + "</p>");
            else logFile?.WriteLine("<p class='" + logClassName + "'><span class='time-tag'>" + GetFormatedElapsedTime() + "</span>" + message + "</p>");
        }
    }
}