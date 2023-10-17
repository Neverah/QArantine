namespace TestFramework.Code
{
    namespace Test
    {
        using TestFramework.Code.FrameworkModules;
        public class TestCase
        {
            public enum TestCaseState 
            {
                Testing,
                Passed,
                Failed
            }

            private string _CurrentStep;
            private TestCaseState _State;

            public FrameworkTest ParentTest { get; }
            public string ID { get; }
            public string CurrentStep{ get => _CurrentStep; }
            public TestCaseState State { get => _State; }
            private readonly HashSet<TestError> TestCaseErrors;

            public TestCase(FrameworkTest parentTest, string ID)
            {
                this.ParentTest = parentTest;
                this.ID = ID;
                _CurrentStep = ParentTest.TestCaseInitStepID;
                _State = TestCaseState.Testing;
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
                    if (HasErrors()) _State = TestCaseState.Failed;
                    else _State = TestCaseState.Passed;
                }
            }

            public bool HasErrors()
            {
                return TestCaseErrors.Count > 0;
            }

            public void AddError(TestError testError)
            {
                if (!TestCaseErrors.Add(testError))
                {
                    LogManager.LogTestWarning($"The error with ErrorType '{testError.ErrorID}' is duplicated, it hasn't been inserted.");
                }
            }

            public override string ToString()
            {
                return ID;
            }

            protected virtual void OnTestCaseStart()
            {
                LogManager.LogOK($"> The TestCase has started: '{this.ID}'");
            }

            protected virtual void OnTestCaseEnd()
            {
                LogManager.LogOK($"> The TestCase has been completed: '{this.ID}'");
            }

            private void ErrorAlreadyExists(TestError newError)
            {

            }
        }
    }
}