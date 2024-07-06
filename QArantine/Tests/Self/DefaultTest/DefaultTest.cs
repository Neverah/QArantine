using QArantine.Code.Test;

namespace QArantine.Tests
{
    public class DefaultTest : QArantineTest
    {

        protected override Dictionary<string, Func<(String, float)>> CreateFlowChart()
        {
            return new Dictionary<string, Func<(String, float)>>
            {
                { "Init", () => 
                    {
                        LogTestOK($"Starting the current TestCase: '{CurrentTestCase}'");
                        return ("Introduction", 0f);
                    } 
                },

                { "Introduction", () => 
                    {
                        LogTestOK($"This is the first TestStep of the current TestCase");
                        LogTestOK($"You should try implementing your own test by creating a flowchart that meets your needs");
                        return ("ErrorReportExample", 0f);
                    } 
                },

                { "ErrorReportExample", () => 
                    {
                        LogTestOK($"This is the second step of the current TestCase");
                        LogTestOK($"An example of a simple error report (without additional fields):");
                        ReportTestError(CreateTestError("EXAMPLE_OF_SIMPLE_ERROR", "DEBUG_EXPECTED_ERROR", "Example of a simple error report (without additional fields)"));
                        LogTestOK($"An example of an extended error report (with additional fields):");
                        ReportTestError(CreateTestError("EXAMPLE_OF_EXTENDED_ERROR", "DEBUG_EXPECTED_ERROR", "Example of an extended error report (with additional fields)")
                            .AddExtraField("Today Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                            .AddExtraField("1 + 1", 1 + 1)
                            .AddExtraField("Value Of PI", Math.PI)
                            .AddExtraField("Roses are", "Blue")
                        );
                        LogTestOK($"Two errors in the same TestCase with all their fields being the same (excluding the TestStep) are considered equal, and duplicates are discarded");
                        ReportTestError(CreateTestError("EXAMPLE_OF_EXTENDED_ERROR", "DEBUG_EXPECTED_ERROR", "Example of an extended error report (with additional fields)")
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
                        LogTestOK($"This is the third step of the current TestCase");
                        LogTestOK($"A 2-second delay will be executed");
                        return ("End", 2f);
                    } 
                },

                { "End", () => 
                    {
                        LogTestOK($"The TestCase has been completed: '{CurrentTestCase}'");
                        return ("", 0f);
                    } 
                },
            };
        }

        protected override void CreateTestCasesList()
        {
            base.CreateTestCasesList();

            TestCasesList.Add(new(this, "TestCaseExample1"));
            TestCasesList.Add(new(this, "TestCaseExample2"));
        }
    }
}