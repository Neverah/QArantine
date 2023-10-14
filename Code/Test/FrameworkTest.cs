namespace TestFramework.Code
{
    namespace Test
    {
        using TestFramework.Code.FrameworkModules;
        using TestFlowGraph = Dictionary<string, Func<(String, int)>>;
        public abstract class FrameworkTest
        {
            public bool Paused { get; set; }
            public float Timeout { get; set; }
            public string State { get; set; }
            public string Name { get; }
            public string CurrentTestCase { get; set; }
            public string CurrentTestStep { get; set; }

            protected TestFlowGraph flowGraph;

            public FrameworkTest()
            {
                this.Name = this.GetType().Name;

                this.Paused = false;
                this.Timeout = 30f;

                this.State = "Executing";
                this.CurrentTestCase = "";
                this.CurrentTestStep = "";

                this.flowGraph = CreateFlowGraph();
            }

            public void Launch()
            {
                LogManager.LogOK($"Comenzando la ejecución del test: {this.Name}");

                OnTestStart();
                StartExecutionLoop();
            }

            protected TestFlowGraph CreateFlowGraph()
            {
                return new TestFlowGraph
                {
                    { "Init", () => 
                        {
                            LogManager.LogOK($"Comenzando el flujo de ejecución del test: {this.Name}");
                            return ("End", 3);
                        } 
                    },

                    { "End", () => 
                        {
                            LogManager.LogOK($"Fin del flujo de ejecución del test: {this.Name}");
                            return ("", 0);
                        } 
                    },
                };
            }

            protected void LoadRequiredData()
            {
                LogManager.LogOK($"Cargando los datos para el test: {this.Name}");

            }
            protected void CreateTestCasesList()
            {

            }

            protected void RecoverStateFromPreviousExecution()
            {

            }

            protected void SetTestPreconditions()
            {

            }

            protected void OnTestStart()
            {

            }

            protected void OnTestEnd()
            {
                
            }

            private void StartExecutionLoop()
            {

            }
        }
    }
}