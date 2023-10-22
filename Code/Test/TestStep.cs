namespace TestFramework.Code
{
    namespace Test
    {
        using TestFramework.Code.FrameworkModules;
        public class TestStep
        {
            public TestCase ParentTestCase { get; }
            public string ID { get; }
            public HashSet<(string, object)> StepFieldsList { get; }

            public TestStep(TestCase parentTestCase, string ID)
            {
                ParentTestCase = parentTestCase;
                this.ID = ID;
                StepFieldsList = new();
            }

            public TestStep AddField((string, object) newField)
            {
                StepFieldsList.Add(newField);
                return this;
            }

            public virtual void OnTestStepStart()
            {
                LogManager.LogOK($"> The TestStep has started: '{ID}'");
            }

            public virtual void OnTestStepEnd()
            {
                LogManager.LogOK($"> The TestStep has been completed: '{ID}'");
            }
        }
    }
}