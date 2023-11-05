using TestFramework.Code.Test;
using TestFramework.Code.FrameworkModules;

namespace TestFramework.FrameworkTests
{
    public class TestLog : FrameworkTest
    {
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
                        LogManager.LogTestOK($"Setting log level to");
                        return ("ErrorReportExample", 0f);
                    } 
                },

                { "ErrorReportExample", () => 
                    {
                        
                        return ("WaitExample", 0f);
                    } 
                },

                { "WaitExample", () => 
                    {

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
                TestCasesList.Add(new(this, lvl.ToString()));
            }
        }
    }
}