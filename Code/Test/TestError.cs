namespace TestFramework.Code
{
    namespace Test
    {
        using TestFramework.Code.FrameworkModules;
        public class TestError
        {
            public string TestName { get; }
            public string TestCaseName { get; }
            public string TestStepName { get; }
            public string ErrorID { get; }
            public List<(string, object)> ExtraFieldsList { get; }

            public TestError(string testName, string testCaseName, string testStepName, string errorID)
            {
                TestName = testName;
                TestCaseName = testCaseName;
                TestStepName = testStepName;
                ErrorID = errorID;
                ExtraFieldsList = new();
            }

            override public string ToString()
            {
                return TestName + ";" + TestCaseName + ";" + TestStepName + ";" + ErrorID;
            }
        }
    }
}