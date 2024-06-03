using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

using TestFramework.Code.FrameworkModules;

namespace TestFramework.Code.FrameworkModules.Logs
{
    public class ConsoleLogHandler : ILogHandler
    {
        private bool WriteOnConsole;

        public static readonly Dictionary<LogManager.LogLevel, ConsoleColor> LogColorConsoleMap = new()
        {
            { LogManager.LogLevel.FatalError, ConsoleColor.DarkRed },
            { LogManager.LogLevel.Error, ConsoleColor.Red },
            { LogManager.LogLevel.OK, ConsoleColor.Green },
            { LogManager.LogLevel.Warning, ConsoleColor.DarkYellow },
            { LogManager.LogLevel.Debug, ConsoleColor.White },
        };

        public ConsoleLogHandler()
        {
            WriteOnConsole = ConfigManager.GetTFConfigParamAsBool("WriteLogsOnConsole");
            StartLogCloseEvent();
        }

        private void StartLogCloseEvent()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, args) => CloseLog();
        }

        private void CloseLog()
        {
            Console.ForegroundColor = ConsoleColor.White;
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

        private void LogGeneric(string message, LogManager.LogLevel logLvl, bool isTestLog = false)
        {
            if (LogManager.LogLvl >= logLvl)
            {
                string[] lines = message.Split('\n');
                foreach (string line in lines)
                {
                    PrintConsoleLogTimePrefix();
                    if (isTestLog) PrintConsoleLogTestPrefix();
                    WriteLog(line, logLvl, isTestLog);
                }
            }
        }

        private void PrintCallStack()
        {
            StackTrace stackTrace = new();

            Console.ForegroundColor = ConsoleColor.White;
            WriteLog("Call Stack:\n{", LogManager.LogLevel.Debug);
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
            WriteLog("\nCall Stack:\n{", LogManager.LogLevel.FatalError);
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                StackFrame? frame = stackTrace.GetFrame(i);
                MethodBase? method = frame?.GetMethod();
                WriteLog($"\t- {method?.DeclaringType}.{method?.Name}", LogManager.LogLevel.FatalError);
            }
            WriteLog("}", LogManager.LogLevel.FatalError);
        }

        private void PrintConsoleLogTimePrefix()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            if(WriteOnConsole) Console.Write(GetFormatedElapsedTime());
        }

        private void PrintConsoleLogTestPrefix()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            if(WriteOnConsole) Console.Write(GetLogTestPrefix());
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
            if(WriteOnConsole)
            {
                Console.ForegroundColor = LogColorConsoleMap[logLvl];
                Console.WriteLine(message);
            }
        }
    }
}