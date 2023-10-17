namespace TestFramework.Code
{
    namespace Test
    {
        using TestFramework.Code.FrameworkModules;
        public class TestStep
        {
            public TestCase ParentTestCase { get; }
            public string ID { get; }

            public TestStep(TestCase parentTestCase, string ID)
            {
                this.ParentTestCase = parentTestCase;
                this.ID = ID;
            }

            protected virtual void OnTestStepStart()
            {
                LogManager.LogOK($"> The TestStep has started: '{this.ID}'");
            }

            protected virtual void OnTestStepEnd()
            {
                LogManager.LogOK($"> The TestStep has been completed: '{this.ID}'");
            }
        }
    }
}