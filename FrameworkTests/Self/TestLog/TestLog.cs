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
                        LogManager.LogTestOK($"You should try implementing your own test by creating a flowchart that meets your needs");
                        return ("ErrorReportExample", 0f);
                    } 
                },

                { "ErrorReportExample", () => 
                    {
                        LogManager.LogTestOK($"This is the second step of the current TestCase");
                        LogManager.LogTestOK($"An example of a simple error report (without additional fields):");
                        ReportTestError(CreateTestError("EXAMPLE_OF_SIMPLE_ERROR"));
                        LogManager.LogTestOK($"An example of an extended error report (with additional fields):");
                        ReportTestError(CreateTestError("EXAMPLE_OF_EXTENDED_ERROR")
                            .AddExtraField("Today Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                            .AddExtraField("1 + 1", 1 + 1)
                            .AddExtraField("Value Of PI", Math.PI)
                            .AddExtraField("Roses are", "Blue")
                        );
                        LogManager.LogTestOK($"Two errors in the same TestCase with all their fields being the same (excluding the TestStep) are considered equal, and duplicates are discarded");
                        ReportTestError(CreateTestError("EXAMPLE_OF_EXTENDED_ERROR")
                            .AddExtraField("Today Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                            .AddExtraField("1 + 1", 1 + 1)
                            .AddExtraField("Value Of PI", Math.PI)
                            .AddExtraField("Roses are", "Blue")
                        );
                        return ("WaitExample", 0f);
                    } 
                },

                { "WaitExample", () => 
                    {
                        LogManager.LogTestOK($"This is the third step of the current TestCase");
                        LogManager.LogTestOK($"A 2-second delay will be executed");
                        return ("End", 2f);
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