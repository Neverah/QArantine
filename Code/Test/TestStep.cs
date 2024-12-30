namespace QArantine.Code
{
    namespace Test
    {
        public class TestStep
        {
            public TestCase ParentTestCase { get; private set;}
            public string ID { get; private set;}
            public int stepNum { get; private set;}
            public HashSet<(string, object)> StepFieldsList { get; } // public so it can be serialized

            public TestStep(TestCase parentTestCase, string ID, int stepNum)
            {
                ParentTestCase = parentTestCase;
                this.ID = ID;
                this.stepNum = stepNum;
                StepFieldsList = [];
            }

            public TestStep AddField((string, object) newField)
            {
                StepFieldsList.Add(newField);
                return this;
            }

            public virtual void OnTestStepStart()
            {
                LogTestDebug($"> The TestStep has started: '{this.ToString()}'");
            }

            public virtual void OnTestStepEnd()
            {
                LogTestDebug($"> The TestStep has been completed: '{this.ToString()}'");
            }

            public override string ToString()
            {
                return stepNum + "-" + ID;
            }
        }
    }
}