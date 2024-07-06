using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

using QArantine.Code.Test;
using QArantine.Code.FrameworkModules;
using QArantine.Code.FrameworkModules.GUI;
using QArantine.Code.QArantineGUI.ViewModels;

namespace QArantine.Tests
{
    public class TestLogSystems : QArantineTest
    {
        private LogManager.LogLevel CurrentCaseLogLevel;
        private int CurrentCaseInitExpectedPrints = 0;

        protected override Dictionary<string, Func<(String, float)>> CreateFlowChart()
        {
            return new Dictionary<string, Func<(String, float)>>
            {
                { "Init", () => 
                    {
                        LogManager.LogLvl = LogManager.LogLevel.Debug;
                        LogTestOK($"Starting the current TestCase: '{CurrentTestCase}'");
                        return ("SetLogLevel", 0f);
                    } 
                },

                { "SetLogLevel", () => 
                    {
                        LogTestOK($"Setting log level to: '{CurrentTestCase}'");
                        if (CurrentTestCase.TryGetDataField("LogLevelValue", out object foundLvl))
                        {
                            CurrentCaseLogLevel = (LogManager.LogLevel)foundLvl;
                            LogManager.LogLvl = CurrentCaseLogLevel;
                        }
                        else
                        {
                            ReportTestError(CreateTestError("NO_LOG_LEVEL_DEFINED", "TEST_ISSUE", $"No 'LogManager.LogLevel' value has been defined for the current TestCase: '{CurrentTestCase}'"));
                            return ("End", 0f);
                        }
                        return ("PrintLogs", 0f);
                    } 
                },

                { "PrintLogs", () => 
                    {
                        LogDebug(GetLogLevelFormatedLog(LogManager.LogLevel.Debug));
                        LogTestDebug(GetLogLevelFormatedLog(LogManager.LogLevel.Debug));

                        LogWarning(GetLogLevelFormatedLog(LogManager.LogLevel.Warning));
                        LogTestWarning(GetLogLevelFormatedLog(LogManager.LogLevel.Warning));

                        LogOK(GetLogLevelFormatedLog(LogManager.LogLevel.OK));
                        LogTestOK(GetLogLevelFormatedLog(LogManager.LogLevel.OK));

                        LogError(GetLogLevelFormatedLog(LogManager.LogLevel.Error));
                        LogTestError(GetLogLevelFormatedLog(LogManager.LogLevel.Error));

                        LogFatalError(GetLogLevelFormatedLog(LogManager.LogLevel.FatalError));
                        LogTestFatalError(GetLogLevelFormatedLog(LogManager.LogLevel.FatalError));

                        return ("CheckLogs", 1f);
                    } 
                },

                { "CheckLogs", () => 
                    {

                        List<string> fileLogLines = new(GetFileLogLines());
                        List<string> errorsFileLogLines = new(GetErrorsFileLogLines());
                        ObservableCollection<LogLine> guiLogLines = new(GetGUILogLines());
                        int expectedPrints = CurrentCaseInitExpectedPrints;

                        foreach (LogManager.LogLevel lvl in Enum.GetValues(typeof(LogManager.LogLevel)))
                        {
                            string expectedLinePattern = ".*" + GetLogLevelFormatedLog(lvl) + ".*";
                            if (lvl == LogManager.LogLevel.None || lvl > CurrentCaseLogLevel)
                            {
                                CheckPrintCountInTheLogFileIsTheExpected(fileLogLines, expectedLinePattern, lvl, 0);
                                CheckPrintCountInTheErrorsLogFileIsTheExpected(fileLogLines, expectedLinePattern, lvl, 0);
                                CheckPrintCountInTheLogConsoleIsTheExpected(guiLogLines, expectedLinePattern, lvl, 0);
                            }
                            else
                            {
                                CheckPrintCountInTheLogFileIsTheExpected(fileLogLines, expectedLinePattern, lvl, expectedPrints);
                                CheckPrintCountInTheErrorsLogFileIsTheExpected(fileLogLines, expectedLinePattern, lvl, expectedPrints);
                                CheckPrintCountInTheLogConsoleIsTheExpected(guiLogLines, expectedLinePattern, lvl, expectedPrints);
                                expectedPrints-=2;
                            }
                        }
                        return ("End", 0f);
                    } 
                },

                { "End", () => 
                    {
                        CurrentCaseInitExpectedPrints += 2;
                        LogTestOK($"The TestCase has been completed: '{CurrentTestCase}'");
                        return ("", 0f);
                    } 
                },
            };
        }

        protected override void CreateTestCasesList()
        {
            base.CreateTestCasesList();

            foreach (LogManager.LogLevel lvl in Enum.GetValues(typeof(LogManager.LogLevel)))
            {
                TestCase newTestCase = new(this, lvl.ToString());
                TestCasesList.Add(newTestCase.AddDataField("LogLevelValue", lvl));
            }
        }

        protected override void PreLaunchChecks()
        {
            while(GUIManager.Instance!.AvaloniaMainWindowViewModel == null || GUIManager.Instance!.AvaloniaMainWindowViewModel.LogLines == null)
            {
                Thread.Sleep(1000);
            }
        }

        private void CheckPrintCountInTheLogFileIsTheExpected(List<string> logLines, string linePattern, LogManager.LogLevel lvl, int expectedCount)
        {
            Regex regex = new Regex(linePattern);
            int printCount = logLines.Count(logLine => regex.IsMatch(logLine));

            if(printCount != expectedCount)
            {
                ReportTestError(CreateTestError("UNEXPECTED_FILE_LOG_COUNT", "TEST_FRAMEWORK", "The ammount of printed file log lines found for the current debug level was not the expected")
                    .AddExtraField("Log level", lvl)
                    .AddExtraField("Expected count", expectedCount)
                    .AddExtraField("Found count", printCount)
                );
            }
        }

        private void CheckPrintCountInTheErrorsLogFileIsTheExpected(List<string> logLines, string linePattern, LogManager.LogLevel lvl, int expectedCount)
        {
            Regex regex = new Regex(linePattern);
            int printCount = logLines.Count(logLine => regex.IsMatch(logLine));

            if(printCount != expectedCount)
            {
                ReportTestError(CreateTestError("UNEXPECTED_ERRORS_FILE_LOG_COUNT", "TEST_FRAMEWORK", "The ammount of printed file log lines found for the current debug level was not the expected")
                    .AddExtraField("Log level", lvl)
                    .AddExtraField("Expected count", expectedCount)
                    .AddExtraField("Found count", printCount)
                );
            }
        }

        private void CheckPrintCountInTheLogConsoleIsTheExpected(ObservableCollection<LogLine> logLines, string linePattern, LogManager.LogLevel lvl, int expectedCount)
        {
            Regex regex = new Regex(linePattern);
            int printCount = logLines.Count(logLine => regex.IsMatch(logLine.ToString()));

            if(printCount != expectedCount)
            {
                ReportTestError(CreateTestError("UNEXPECTED_CONSOLE_LOG_COUNT", "TEST_FRAMEWORK", "The ammount of printed console log lines found for the current debug level was not the expected")
                    .AddExtraField("Log level", lvl)
                    .AddExtraField("Expected count", expectedCount)
                    .AddExtraField("Found count", printCount)
                );
            }
        }

        private string GetLogLevelFormatedLog(LogManager.LogLevel logLvl)
        {
            return $"This is a '{logLvl}' level example log for the TestLogSystems execution";
        }

        private List<string> GetFileLogLines()
        {
            return ReadLogFile(LogManager.fileLogHandler.LogPath);
        }

        private List<string> GetErrorsFileLogLines()
        {
            return ReadLogFile(LogManager.fileLogHandler.ErrorsLogPath);
        }

        private ObservableCollection<LogLine> GetGUILogLines()
        {
            return GUIManager.Instance!.AvaloniaMainWindowViewModel!.LogLines;
        }

        private List<string> ReadLogFile(string filePath)
        {
            List<string> lines = new List<string>();
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        if (reader != null)
                        {
                            string? line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                lines.Add(line);
                            }
                        }
                        else
                        {
                            LogFatalError($"An error happened when trying to create the StreamReder to read the '{filePath}' log file");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogFatalError($"An error happened when trying to read the '{filePath}' log file: {ex.Message}");
            }

            return lines;
        }
    }
}