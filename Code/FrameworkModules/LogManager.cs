using System.Diagnostics;
using System.Reflection;

namespace TestFramework.Code.FrameworkModules
{
    public static class LogManager
    {
        private static string LogPath = TestManager.OUTPUT_DIRECTORY + "/Log";
        private static bool DumpToLogFile = false;
        private static StreamWriter? LogFile;

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

        static LogManager() => LogLvl = LogLevel.Debug;

        public static void LogError(string message)
        {
            if (LogLvl >= LogLevel.Error)
            {
                PrintConsoleLogTestPrefix();
                Console.ForegroundColor = ConsoleColor.Red;
                WriteTestLog($"[ERROR] {message}", LogLevel.Error);
            }

            if (LogLvl >= LogLevel.Debug) PrintCallStack();
        }

        public static void LogOK(string message)
        {
            if (LogLvl >= LogLevel.OK)
            {
                PrintConsoleLogTestPrefix();
                Console.ForegroundColor = ConsoleColor.Green;
                WriteTestLog($"[OK] {message}", LogLevel.OK);
            }
        }

        public static void LogWarning(string message)
        {
            if (LogLvl >= LogLevel.Warning)
            {
                PrintConsoleLogTestPrefix();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                WriteTestLog($"[WARNING] {message}", LogLevel.Warning);
            }
        }

        public static void LogDebug(string message)
        {
            if (LogLvl >= LogLevel.Debug)
            {
                PrintConsoleLogTestPrefix();
                Console.ForegroundColor = ConsoleColor.White;
                WriteTestLog($"[DEBUG] {message}", LogLevel.Debug);
            }
        }

        private static void PrintCallStack()
        {
            StackTrace stackTrace = new();

            Console.ForegroundColor = ConsoleColor.White;
            WriteTestLog("Call Stack:\n{", LogLevel.Debug, false);
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                StackFrame? frame = stackTrace.GetFrame(i);
                MethodBase? method = frame.GetMethod();
                WriteTestLog($"\t- {method?.DeclaringType}.{method?.Name}", LogLevel.Debug, false);
            }
            WriteTestLog("}", LogLevel.Debug, false);
        }

        private static void PrintConsoleLogTestPrefix()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(GetFormatedElapsedTime());
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

        public static void StartTestLogFile(string testClassName)
        {
            if (testClassName != null) LogPath = LogPath + "_" + testClassName + ".html";
            else LogPath += ".html";

            DeleteOldTestLogFile();
            CreateTestLogFile();
            DumpToLogFile = true;
        }

        public static void CloseTestLogFile()
        {
            LogFile?.WriteLine("<p class='ok'><span class='time-tag'>" + GetFormatedElapsedTime() + "</span>Cerrando el log...</p>");
            LogFile?.WriteLine("</body>");
            LogFile?.WriteLine("</html>");
            LogFile?.Close();
        }

        private static void DeleteOldTestLogFile()
        {
            if (File.Exists(LogPath)) File.Delete(LogPath);
        }

        private static void CreateTestLogFile()
        {
            LogFile = new(LogPath, append: true);

            string htmlHeader = @"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    /* Estilo para los párrafos */
                    p {
                        margin: 5px 0;
                        font-size: 14px;
                        font-weight: bold;
                        font-family: 'Arial', sans-serif;
                    }
                    /* Estilo del fondo */
                    body { background-color: #111; color: white; }
                    /* Estilos para los diferentes tags de logs */
                    .error { color: rgb(220, 69, 69); }
                    .ok { color: rgb(20, 140, 20); }
                    .warning { color: rgb(220, 180, 80) }
                    .debug { color: rgb(230, 230, 230); }
                    .test-log-prefix { color: rgb(100, 100, 240); }
                    .time-tag { color: rgb(160, 160, 160); }
                </style>
            </head>
            <body>";
            LogFile.WriteLine(htmlHeader);

            // Manejador de eventos para cerrar el log en el cierre de la aplicación
            AppDomain.CurrentDomain.ProcessExit += (sender, args) => CloseTestLogFile();
        }

        private static void WriteTestLog(string message, LogLevel lvl, bool printPrefixOnFile = true)
        {
            Console.WriteLine(message);
            if (DumpToLogFile) WriteTestLogOnFile(message, lvl, printPrefixOnFile);
        }

        private static void WriteTestLogOnFile(string message, LogLevel lvl, bool printPrefix)
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
                    message = "[ERROR: ESTE LOG NO TIENE NINGÚN TIPO DEFINIDO] - " + message;
                    break;
            }

            if (printPrefix) LogFile?.WriteLine("<p class='" + logClassName + "'><span class='time-tag'>" + GetFormatedElapsedTime() + "</span><span class='test-log-prefix'>" + GetLogTestPrefix(true) + "</span> " + message + "</p>");
            else LogFile?.WriteLine("<p class='" + logClassName + "'><span class='time-tag'>" + GetFormatedElapsedTime() + "</span>" + message + "</p>");
        }
    }
}