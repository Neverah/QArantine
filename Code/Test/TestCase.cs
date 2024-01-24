using System.Collections.Generic;

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

            private TestStep _CurrentStep;
            private TestCaseState _State;
            private Dictionary<string, object> TestCaseData { get; }

            public FrameworkTest ParentTest { get; }
            public string ID { get; }
            public TestStep CurrentStep{ get => _CurrentStep; }
            public TestCaseState State { get => _State; }
            public HashSet<TestError> TestCaseErrors { get; }

            public TestCase(FrameworkTest parentTest, string ID)
            {
                ParentTest = parentTest;
                this.ID = ID;
                _CurrentStep = new(this, ParentTest.TestCaseInitStepID);
                _State = TestCaseState.Testing;
                TestCaseData = new();
                TestCaseErrors = new();
            }

            public void UpdateTestStep(string nextTestStepName)
            {
                if (nextTestStepName != null && nextTestStepName != "")
                {
                    if (nextTestStepName != CurrentStep.ID)
                    {
                        _CurrentStep.OnTestStepEnd();
                        _CurrentStep = new(this, nextTestStepName);
                        _CurrentStep.OnTestStepStart();
                    }
                }
                else
                {
                    _CurrentStep.OnTestStepEnd();
                    if (HasErrors()) _State = TestCaseState.Failed;
                    else _State = TestCaseState.Passed;
                }
            }

            public object? GetDataField(string dataFieldID)
            {
                TestCaseData.TryGetValue(dataFieldID, out object? foundValue);
                return foundValue;
            }

            public bool TryGetDataField(string dataFieldID, out object value)
            {
                if(TestCaseData.TryGetValue(dataFieldID, out value!))
                {
                    return true;
                }
                else
                {
                    value = null!;
                    return false;
                }
            }

            public TestCase AddDataField(string dataFieldID, object dataFieldValue)
            {
                TestCaseData.Add(dataFieldID, dataFieldValue);
                return this;
            }

            public virtual bool HasErrors()
            {
                return TestCaseErrors.Count > 0;
            }

            public virtual void AddError(TestError testError)
            {
                if (!TestCaseErrors.Add(testError))
                {
                    LogManager.LogTestWarning($"The last reported error with ErrorType '{testError.ErrorID}' is duplicated, it hasn't been stored.");
                }
            }

            public virtual void LogFoundErrors()
            {
                foreach(TestError testError in TestCaseErrors)
                {
                    LogManager.LogError("- " + testError.ToString());
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
        }
    }
}