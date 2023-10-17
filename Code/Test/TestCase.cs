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
            private readonly HashSet<TestError> TestCaseErrors;

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

            public bool HasErrors()
            {
                return TestCaseErrors.Count > 0;
            }

            public void AddError(TestError testError)
            {
                if (!TestCaseErrors.Add(testError))
                {
                    LogManager.LogTestWarning($"El error con ErrorType '{testError.ErrorID}' introducido esta duplicado, no se ha insertado");
                }
            }

            public override string ToString()
            {
                return ID;
            }

            protected virtual void OnTestCaseStart()
            {
                LogManager.LogOK($"> Ha comenzado el TestCase: '{this.ID}'");
            }

            protected virtual void OnTestCaseEnd()
            {
                LogManager.LogOK($"> Ha finalizado el TestCase: '{this.ID}'");
            }

            private void ErrorAlreadyExists(TestError newError)
            {

            }
        }
    }
}