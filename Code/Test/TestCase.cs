namespace TestFramework.Code
{
    namespace Test
    {
        using TestFramework.Code.FrameworkModules;
        public class TestCase
        {
            public enum TestCaseStatus 
            {
                Testing,
                Passed,
                Failed
            }

            private string _CurrentStep;
            private TestCaseStatus _Status;

            public FrameworkTest ParentTest { get; }
            public string ID { get; }
            public string CurrentStep{ get => _CurrentStep; }
            public TestCaseStatus Status { get => _Status; }
            public List<TestError> TestCaseErrors { get; }

            public TestCase(FrameworkTest parentTest, string ID)
            {
                this.ParentTest = parentTest;
                this.ID = ID;
                _CurrentStep = ParentTest.TestCaseInitStepID;
                _Status = TestCaseStatus.Testing;
                TestCaseErrors = new();
            }

            public void UpdateTestStep(string nextTestStepName)
            {
                if (nextTestStepName != null && nextTestStepName != "")
                {
                    _CurrentStep = nextTestStepName;
                }
                else
                {
                    if (HasErrors()) _Status = TestCaseStatus.Failed;
                    else _Status = TestCaseStatus.Passed;
                }
            }

            protected void OnTestCaseStart()
            {
                LogManager.LogOK($"> Ha comenzado el TestCase: '{this.ID}'");
            }

            protected void OnTestCaseEnd()
            {
                LogManager.LogOK($"> Ha finalizado el TestCase: '{this.ID}'");
            }

            public bool HasErrors()
            {
                return TestCaseErrors.Count > 0;
            }
        }
    }
}