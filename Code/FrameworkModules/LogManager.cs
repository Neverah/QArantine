using System.Diagnostics;
using System.Reflection;

namespace TestFramework.Code.FrameworkModules
{
    public static class LogManager
    {
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

        static LogManager() => LogManager.LogLvl = LogLevel.Debug;

        public static void LogError(String message)
        {
            if (LogLvl >= LogLevel.Error)
            {
                PrintLogTestPrefix();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {message}");
            }

            if (LogLvl >= LogLevel.Debug) PrintCallStack();
        }

        public static void LogOK(String message)
        {
            if (LogLvl >= LogLevel.OK)
            {
                PrintLogTestPrefix();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[OK] {message}");
            }
        }

        public static void LogWarning(String message)
        {
            if (LogLvl >= LogLevel.Warning)
            {
                PrintLogTestPrefix();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"[WARNING] {message}");
            }
        }

        public static void LogDebug(String message)
        {
            if (LogLvl >= LogLevel.Debug)
            {
                PrintLogTestPrefix();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[DEBUG] {message}");
            }
        }

        private static void PrintCallStack()
        {
            StackTrace stackTrace = new();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Call Stack:\n{");
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                StackFrame? frame = stackTrace.GetFrame(i);
                MethodBase? method = frame.GetMethod();
                Console.WriteLine($"\t- {method?.DeclaringType}.{method?.Name}");
            }
            Console.WriteLine("}");
        }

        private static void PrintLogTestPrefix()
        {
            if (TestManager?.CurrentTest != null && TestManager?.CurrentTest?.CurrentTestCase != "")
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.Write($"<{TestManager?.CurrentTest.CurrentTestCase}:{TestManager?.CurrentTest.CurrentTestStep}>");
            }
        }
    }
}