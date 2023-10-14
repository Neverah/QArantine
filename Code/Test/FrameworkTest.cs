namespace TestFramework.Code
{
    namespace Test
    {
        using TestFramework.Code.FrameworkModules;
        using TestFlowGraph = Dictionary<string, Func<(String, float)>>;
        public abstract class FrameworkTest
        {
            public enum TestStatus
            {
                Testing,
                Passed,
                Failed
            }

            public bool Paused { get; set; }
            public float Timeout { get; set; }
            public TestStatus Status { get; set; }
            public string Name { get; }
            public TestCase? CurrentTestCase { get; set; }
            public string TestCaseInitStepID { get; }
            public string TestCaseEndStepID { get; }

            private readonly TestFlowGraph FlowGraph;
            private readonly List<TestCase> TestCasesList;

            public FrameworkTest()
            {
                Name = GetType().Name;

                Paused = false;
                Timeout = 30f;

                Status = TestStatus.Testing;

                TestCaseInitStepID = "Init";
                TestCaseEndStepID = "End";

                FlowGraph = CreateFlowGraph();
                TestCasesList = new();
            }

            public void Launch()
            {
                LoadRequiredData();
                CreateTestCasesList();
                RecoverStatusFromPreviousExecution();
                SetTestPreconditions();
                OnTestStart();
                ExecutionLoop();
            }

            public bool HasErrors()
            {
                foreach(TestCase testCase in TestCasesList)
                {
                    if (testCase.HasErrors()) return true;
                }
                return false;
            }

            protected TestFlowGraph CreateFlowGraph()
            {
                return new TestFlowGraph
                {
                    { "Init", () => 
                        {
                            LogManager.LogOK($"Comenzando el caso de prueba: '{CurrentTestCase}'");
                            return ("Step1", 1f);
                        } 
                    },

                    { "Step1", () => 
                        {
                            LogManager.LogOK($"Esto es el Step1");
                            return ("Step2", 1f);
                        } 
                    },

                    { "Step2", () => 
                        {
                            LogManager.LogOK($"Esto es el Step2");
                            return ("End", 2f);
                        } 
                    },

                    { "End", () => 
                        {
                            LogManager.LogOK($"Ha terminado el caso de prueba: '{CurrentTestCase}'");
                            return ("", 0f);
                        } 
                    },
                };
            }

            protected void LoadRequiredData()
            {
                LogManager.LogOK($"> Cargando los datos para el test: '{this.Name}'");

            }
            protected void CreateTestCasesList()
            {
                LogManager.LogOK($"> Creando la lista de casos de prueba del test: '{this.Name}'");

                TestCasesList.Add(new(this, "TestCaseDePrueba"));
            }

            protected void RecoverStatusFromPreviousExecution()
            {
                LogManager.LogOK($"> Recuperando el estado de una posible ejecución anterior: '{this.Name}'");
            }

            protected void SetTestPreconditions()
            {
                LogManager.LogOK($"> Estableciendo las precondiciones del test: '{this.Name}'");
            }

            protected void OnTestStart()
            {
                LogManager.LogOK($"> Iniciando el test: '{this.Name}'");
            }

            protected void OnTestEnd()
            {
                if (HasErrors()) Status = TestStatus.Failed;
                else Status = TestStatus.Passed;

                LogManager.LogOK($"> Se han comprobado todos los casos de prueba del test: '{this.Name}'");
            }

            private void ExecutionLoop()
            {
                LogManager.LogOK($"> Van a comenzar a probarse todos los casos de prueba del test: '{this.Name}'");

                CurrentTestCase = GetNextTestCase();
                while (CurrentTestCase != null)
                {
                    if (CurrentTestCase.Status == TestCase.TestCaseStatus.Testing)
                    {
                        string previousStep = CurrentTestCase.CurrentStep;

                        (string nextStep, float delay) = FlowGraph[previousStep]();
                        CurrentTestCase.UpdateTestStep(nextStep);

                        if (delay >= 0) Thread.Sleep((int)delay * 1000);
                    }
                    else
                    {
                        CurrentTestCase = GetNextTestCase();
                    }
                }
                // Cuando ya no queden casos de prueba, el test habrá terminado
                OnTestEnd();
            }

            private TestCase? GetNextTestCase()
            {
                foreach (TestCase testCase in TestCasesList)
                {
                    if (testCase.Status == TestCase.TestCaseStatus.Testing)
                    {
                        return testCase;
                    }
                }
                return null;
            }
        }
    }
}