using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

using TestFramework.Code.FrameworkModules;

namespace TestFramework.Code.FrameworkModules.Logs
{
    public class FileLogHandler : ILogHandler
    {
        public readonly Dictionary<LogManager.LogLevel, string> LogClassHTMLFileMap = new()
        {
            { LogManager.LogLevel.FatalError, "fatal-error" },
            { LogManager.LogLevel.Error, "error" },
            { LogManager.LogLevel.OK, "ok" },
            { LogManager.LogLevel.Warning, "warning" },
            { LogManager.LogLevel.Debug, "debug" },
        };

        public string LogPath { get; private set; }
        public string ErrorsLogPath { get; private set; }
        public bool LogsOpen { get; private set; }
        private bool DumpToLogFiles;
        private bool HasLogFileDump;
        private bool HasErrorLogFileDump;
        private StreamWriter? LogFile;
        private StreamWriter? ErrorsLogFile;

        public FileLogHandler()
        {
            LogPath = "";
            ErrorsLogPath = "";

            LogsOpen = false;
            DumpToLogFiles = false;

            HasLogFileDump = ConfigManager.GetTFConfigParamAsBool("DumpLogsToFile");
            if (HasLogFileDump) HasErrorLogFileDump = ConfigManager.GetTFConfigParamAsBool("ErrorsLogActive");

            StartLogFile();
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

        public void StartLogFile()
        {
            if(!LogsOpen)
            {
                if (ThisExecutionHasLogFileDump())
                {
                    DeleteOldLogFiles();
                    CreateLogFiles();
                    LogsOpen = true;
                    DumpToLogFiles = true;

                    StartLogCloseEvent();
                }
            }
        }

        public string GetLogPath()
        {
            InitLogPath();
            return LogPath;
        }

        public string GetErrorsLogPath()
        {
            InitErrorsLogPath();
            return ErrorsLogPath;
        }

        public bool IsLogFileDumpActive()
        {
            return DumpToLogFiles;
        }

        public bool ThisExecutionHasLogFileDump()
        {
            return HasLogFileDump;
        }

        public bool ThisExecutionHasErrorLogFileDump()
        {
            return HasErrorLogFileDump;
        }

        private void CloseLog()
        {
            CloseLogFiles();
        }

        private void CloseLogFiles()
        {
            DumpToLogFiles = false;
            CloseLogFile();
            CloseErrorsLogFile();
            LogsOpen = false;
        }

        private void CloseLogFile()
        {
            if (LogsOpen)
            {
                LogFile?.WriteLine("<p class='ok'><span class='time-tag'>" + GetFormatedElapsedTime() + "</span>Closing the log...</p>");
                LogFile?.WriteLine("</body>");
                LogFile?.WriteLine("</html>");
                LogFile?.Close();
            }
        }

        private void CloseErrorsLogFile()
        {
            if (LogsOpen && ThisExecutionHasErrorLogFileDump())
            {
                ErrorsLogFile?.WriteLine("<p class='ok'><span class='time-tag'>" + GetFormatedElapsedTime() + "</span>Closing the errors log...</p>");
                ErrorsLogFile?.WriteLine("</body>");
                ErrorsLogFile?.WriteLine("</html>");
                ErrorsLogFile?.Close();
            }
        }

        private void InitLogPath()
        {
            if (LogPath != null && LogPath != "") return;

            if ((LogPath = ConfigManager.GetTFConfigParamAsString("LogPath")!) == null)
            {
                LogError("Could not find the 'LogPath' config param. The log can not be opened, aborting execution");
                Environment.Exit(-1);
            }
            
            if (!Path.IsPathRooted(LogPath)) LogPath = Path.Combine(Environment.CurrentDirectory, LogPath);
        }

        private void InitErrorsLogPath()
        {
            if (ErrorsLogPath != null && ErrorsLogPath != "") return;

            if (ThisExecutionHasErrorLogFileDump())
            {
                if ((ErrorsLogPath = ConfigManager.GetTFConfigParamAsString("ErrorsLogPath")!) == null)
                {
                    LogError("Could not find the 'ErrorsLogPath' config param, but the 'ErrorsLogActive' config param is set to 'true'. The errors log can not be opened, aborting execution");
                    Environment.Exit(-1);
                }
                
                if (!Path.IsPathRooted(ErrorsLogPath)) ErrorsLogPath = Path.Combine(Environment.CurrentDirectory, ErrorsLogPath);
            }
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

            WriteLog("\nCall Stack:\n{", LogManager.LogLevel.FatalError);
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
                return $"&lt;[T]:{LogManager.TestManager?.CurrentTest.CurrentTestCase.ID}:{LogManager.TestManager?.CurrentTest.CurrentTestCase.CurrentStep}&gt; ";
            }
            return "";
        }

        private string GetFormatedElapsedTime()
        {
            return "|" + TimeManager.GetAppElapsedSecondsAsString() + "| ";
        }

        private void DeleteOldLogFiles()
        {
            if (File.Exists(GetLogPath())) File.Delete(GetLogPath());
            if (File.Exists(GetErrorsLogPath())) File.Delete(GetErrorsLogPath());
        }

        private void CreateLogFiles()
        {
            CreateLogFile();
            CreateErrorsLogFile();
        }

        private void CreateLogFile()
        {
            CreateGenericLogFile(GetLogPath(), out LogFile);
        }

        private void CreateErrorsLogFile()
        {
            if (ThisExecutionHasErrorLogFileDump()) CreateGenericLogFile(GetErrorsLogPath(), out ErrorsLogFile);
        }

        private void CreateGenericLogFile(string logPath, out StreamWriter outLogWriter)
        {
            outLogWriter = new(logPath, append: true) { AutoFlush = ConfigManager.GetTFConfigParamAsBool("LogsAutoFlush") };

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
                    .fatal-error { color: rgb(180, 65, 65); }
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

        private void StartLogCloseEvent()
        {
            // Event handler for closing the log on program exit
            AppDomain.CurrentDomain.ProcessExit += (sender, args) => CloseLog();
        }

        private void WriteLog(string message, LogManager.LogLevel logLvl, bool printPrefixOnFile = false)
        {   
            if (DumpToLogFiles && LogsOpen) 
            {
                WriteLogOnLogFiles(message, logLvl, printPrefixOnFile);
            }
        }

        private void WriteLogOnLogFiles(string message, LogManager.LogLevel lvl, bool printPrefix)
        {
            WriteLogOnLogFile(LogFile!, message, lvl, printPrefix);
            if(lvl <= LogManager.LogLevel.Error && ThisExecutionHasErrorLogFileDump()) WriteLogOnLogFile(ErrorsLogFile!, message, lvl, false);
        }

        private void WriteLogOnLogFile(StreamWriter logFile, string message, LogManager.LogLevel lvl, bool printPrefix)
        {
            message = message.Replace("<", "&lt;").Replace(">", "&gt;");

            string logClassName;
            if (!LogClassHTMLFileMap.TryGetValue(lvl, out logClassName!))
            {
                logClassName = "error";
                message = "[ERROR: THIS LOG HAS NO DEFINED TYPE] - " + message;
            }

            logFile?.WriteLine(GetFinalLogLineString(message, logClassName, printPrefix));
        }

        internal string GetFinalLogLineString(string message, string logClassName, bool printPrefix)
        {
            if (printPrefix) return "<p class='" + logClassName + "'><span class='time-tag'>" + GetFormatedElapsedTime() + "</span><span class='test-log-prefix'>" + GetLogTestPrefix() + "</span> " + message + "</p>";
            return "<p class='" + logClassName + "'><span class='time-tag'>" + GetFormatedElapsedTime() + "</span>" + message + "</p>";
        }
    }
}