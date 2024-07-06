using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using QArantine.Code.FrameworkModules.Logs;

namespace QArantine.Code.FrameworkModules
{
    public static class LogManager
    {
        public enum LogLevel
        {
            None,
            FatalError,
            Error,
            OK,
            Warning,
            Debug
        }

        public static ConsoleLogHandler consoleLogHandler;
        public static FileLogHandler fileLogHandler;
        public static GUILogHandler guiLogHandler;

        public static LogLevel LogLvl { get; set; }
        public static TestManager? TestManager { get; set; }

        private static bool abortOnFatalError;

        static LogManager()
        {
            LogLvl = LogLevel.Warning;

            consoleLogHandler = new ConsoleLogHandler();
            fileLogHandler = new FileLogHandler();
            guiLogHandler = new GUILogHandler();

            InitLogLevel();

            abortOnFatalError = ConfigManager.GetTFConfigParamAsBool("AbortOnFatalError");

            PrintInitLogMessage();
        }

        private static void PrintInitLogMessage()
        {
            LogOK($"Log start time: {DateTime.Now.ToString("yyyy-MM-dd - HH:mm:ss")}");
            LogOK($"Hostname: {Environment.MachineName}");
            LogOK($"Init LogLevel: {LogLvl.ToString()}");

            string processorArchitecture = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
            LogOK($"OS: {Environment.OSVersion.Platform.ToString()} - {Environment.OSVersion.Version.ToString()} - {processorArchitecture}");
            LogOK($".NET Version: {Environment.Version.ToString()}");
            LogOK($"==============================");
        }

        public static void LogFatalError(string message)
        {
            consoleLogHandler?.LogFatalError(message);
            fileLogHandler?.LogFatalError(message);
            guiLogHandler?.LogFatalError(message);
            if (abortOnFatalError) Environment.Exit(-1);
        }

        public static void LogError(string message)
        {
            consoleLogHandler?.LogError(message);
            fileLogHandler?.LogError(message);
            guiLogHandler?.LogError(message);
        }

        public static void LogOK(string message)
        {
            consoleLogHandler?.LogOK(message);
            fileLogHandler?.LogOK(message);
            guiLogHandler?.LogOK(message);
        }

        public static void LogWarning(string message)
        {
            consoleLogHandler?.LogWarning(message);
            fileLogHandler?.LogWarning(message);
            guiLogHandler?.LogWarning(message);
        }

        public static void LogDebug(string message)
        {
            consoleLogHandler?.LogDebug(message);
            fileLogHandler?.LogDebug(message);
            guiLogHandler?.LogDebug(message);
        }

        public static void LogTestFatalError(string message)
        {
            consoleLogHandler.LogTestFatalError(message);
            fileLogHandler.LogTestFatalError(message);
            guiLogHandler.LogTestFatalError(message);
        }

        public static void LogTestError(string message)
        {
            consoleLogHandler.LogTestError(message);
            fileLogHandler.LogTestError(message);
            guiLogHandler.LogTestError(message);
        }

        public static void LogTestOK(string message)
        {
            consoleLogHandler.LogTestOK(message);
            fileLogHandler.LogTestOK(message);
            guiLogHandler.LogTestOK(message);
        }

        public static void LogTestWarning(string message)
        {
            consoleLogHandler.LogTestWarning(message);
            fileLogHandler.LogTestWarning(message);
            guiLogHandler.LogTestWarning(message);
        }

        public static void LogTestDebug(string message)
        {
            consoleLogHandler.LogTestDebug(message);
            fileLogHandler.LogTestDebug(message);
            guiLogHandler.LogTestDebug(message);
        }

        private static void InitLogLevel()
        {
            string logLvlName;
            if ((logLvlName = ConfigManager.GetTFConfigParamAsString("LogLevel")!) == null)
            {
                LogError("Could not find the 'LogLevel' config param, the log can not be opened, aborting execution");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not find the 'LogLevel' config param, the log can not be opened, aborting execution");
                Console.ForegroundColor = ConsoleColor.White;

                Environment.Exit(-1);
            }

            LogLvl = (LogLevel)Enum.Parse(typeof(LogLevel), logLvlName);
        }
    }
}