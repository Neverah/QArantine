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
                LogManager.LogOK($"> Ha comenzado el TestStep: '{this.ID}'");
            }

            protected virtual void OnTestStepEnd()
            {
                LogManager.LogOK($"> Ha finalizado el TestStep: '{this.ID}'");
            }
        }
    }
}