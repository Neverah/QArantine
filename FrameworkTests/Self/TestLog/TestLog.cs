using TestFramework.Code.Test;
using TestFramework.Code.FrameworkModules;

namespace TestFramework.FrameworkTests
{
    public class TestLog : FrameworkTest
    {
        private LogManager.LogLevel CurrentCaseLogLevel;

        protected override Dictionary<string, Func<(String, float)>> CreateFlowChart()
        {
            return new Dictionary<string, Func<(String, float)>>
            {
                { "Init", () => 
                    {
                        LogManager.LogTestOK($"Starting the current TestCase: '{CurrentTestCase}'");
                        return ("SetLogLevel", 0f);
                    } 
                },

                { "SetLogLevel", () => 
                    {
                        LogManager.LogTestOK($"Setting log level to: '{CurrentTestCase}'");
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
                        LogManager.LogDebug(GetLogLevelFormatedLog(LogManager.LogLevel.Debug));
                        LogManager.LogTestDebug(GetLogLevelFormatedLog(LogManager.LogLevel.Debug));

                        LogManager.LogWarning(GetLogLevelFormatedLog(LogManager.LogLevel.Warning));
                        LogManager.LogTestWarning(GetLogLevelFormatedLog(LogManager.LogLevel.Warning));

                        LogManager.LogOK(GetLogLevelFormatedLog(LogManager.LogLevel.OK));
                        LogManager.LogTestOK(GetLogLevelFormatedLog(LogManager.LogLevel.OK));

                        LogManager.LogError(GetLogLevelFormatedLog(LogManager.LogLevel.Error));
                        LogManager.LogTestError(GetLogLevelFormatedLog(LogManager.LogLevel.Error));

                        LogManager.LogFatalError(GetLogLevelFormatedLog(LogManager.LogLevel.FatalError));
                        LogManager.LogTestFatalError(GetLogLevelFormatedLog(LogManager.LogLevel.FatalError));

                        return ("CheckLogs", 0f);
                    } 
                },

                { "CheckLogs", () => 
                    {
                        switch(CurrentCaseLogLevel)
                        {
                            case LogManager.LogLevel.Debug:
                                LogManager.LogDebug(GetLogLevelFormatedLog(LogManager.LogLevel.Debug));
                                LogManager.LogTestDebug(GetLogLevelFormatedLog(LogManager.LogLevel.Debug));
                                goto case LogManager.LogLevel.Warning;

                            case LogManager.LogLevel.Warning:
                                LogManager.LogDebug(GetLogLevelFormatedLog(LogManager.LogLevel.Warning));
                                LogManager.LogTestDebug(GetLogLevelFormatedLog(LogManager.LogLevel.Warning));
                                goto case LogManager.LogLevel.OK;

                            case LogManager.LogLevel.OK:
                                LogManager.LogDebug(GetLogLevelFormatedLog(LogManager.LogLevel.OK));
                                LogManager.LogTestDebug(GetLogLevelFormatedLog(LogManager.LogLevel.OK));
                                goto case LogManager.LogLevel.Error;

                            case LogManager.LogLevel.Error:
                                LogManager.LogDebug(GetLogLevelFormatedLog(LogManager.LogLevel.Error));
                                LogManager.LogTestDebug(GetLogLevelFormatedLog(LogManager.LogLevel.Error));
                                goto case LogManager.LogLevel.FatalError;

                            case LogManager.LogLevel.FatalError:
                                LogManager.LogDebug(GetLogLevelFormatedLog(LogManager.LogLevel.FatalError));
                                LogManager.LogTestDebug(GetLogLevelFormatedLog(LogManager.LogLevel.FatalError));
                                goto case LogManager.LogLevel.FatalError;

                            case LogManager.LogLevel.None:
                            break;
                        }
                        return ("End", 0f);
                    } 
                },

                { "End", () => 
                    {
                        LogManager.LogTestOK($"The TestCase has been completed: '{CurrentTestCase}'");
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

        private string GetLogLevelFormatedLog(LogManager.LogLevel logLvl)
        {
            return $"This is a '{logLvl}' level log";
        }
    }
}