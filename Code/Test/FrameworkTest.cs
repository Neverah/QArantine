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

            public float Timeout = 60f;
            public bool Paused { get; set; }
            public TestStatus Status { get; set; }
            public string Name { get; }
            public TestCase? CurrentTestCase { get; set; }
            public string TestCaseInitStepID { get; set; }
            public string TestCaseEndStepID { get; set; }

            private CancellationToken TimeoutToken;
            private readonly TestFlowGraph FlowGraph;
            private readonly List<TestCase> TestCasesList;

            public FrameworkTest()
            {
                Name = GetType().Name;

                TestCaseInitStepID = "Init";
                TestCaseEndStepID = "End";

                Paused = false;
                Status = TestStatus.Testing;

                FlowGraph = CreateFlowGraph();
                TestCasesList = new();
            }

            public void Launch(CancellationToken timeoutToken)
            {
                TimeoutToken = timeoutToken;

                LoadRequiredData();
                CreateTestCasesList();
                RecoverStatusFromPreviousExecution();
                SetTestPreconditions();
                OnTestStart();
                ExecutionLoop();
            }

            public TestError CreateTestError(string errorID)
            {
                return new(Name, CurrentTestCase != null ? CurrentTestCase.ID : "UNKNOWN_TEST_CASE", CurrentTestCase != null ? CurrentTestCase.CurrentStep : "UNKNOWN_TEST_STEP", errorID);
            }

            public void ReportTestError(TestError newError)
            {
                LogManager.LogTestError($"TEST ERROR DETECTED > {newError}");
                CurrentTestCase?.AddError(newError);
            }

            protected bool HasErrors()
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
                            LogManager.LogTestOK($"Comenzando el caso de prueba: '{CurrentTestCase}'");
                            return ("Introduction", 0f);
                        } 
                    },

                    { "Introduction", () => 
                        {
                            LogManager.LogTestOK($"Este es el primer Step del caso de prueba");
                            LogManager.LogTestOK($"Deberías probar a implementar tu propio test creandole un grafo de flujo que cumpla tus necesidades");
                            return ("ErrorReportExample", 0f);
                        } 
                    },

                    { "ErrorReportExample", () => 
                        {
                            LogManager.LogTestOK($"Este es el segundo Step del caso de prueba");
                            LogManager.LogTestOK($"Se va a reportar un error simple sin campos adicionales como ejemplo:");
                            ReportTestError(CreateTestError("EXAMPLE_OF_SIMPLE_ERROR"));
                            LogManager.LogTestOK($"Se va a reportar un error extendido con campos adicionales como ejemplo:");
                            ReportTestError(CreateTestError("EXAMPLE_OF_EXTENDED_ERROR")
                                .AddExtraField(("Today Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")))
                                .AddExtraField(("1 + 1", 1 + 1))
                                .AddExtraField(("Value Of PI", Math.PI))
                                .AddExtraField(("Roses are", "Blue"))
                            );
                            LogManager.LogTestOK($"Dos errores en un mismo TestCase y con todos sus campos iguales (ignorando el Step) se consideran iguales, y se descartan duplicados:");
                            ReportTestError(CreateTestError("EXAMPLE_OF_EXTENDED_ERROR")
                                .AddExtraField(("Today Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")))
                                .AddExtraField(("1 + 1", 1 + 1))
                                .AddExtraField(("Value Of PI", Math.PI))
                                .AddExtraField(("Roses are", "Blue"))
                            );
                            return ("WaitExample", 0f);
                        } 
                    },

                    { "WaitExample", () => 
                        {
                            LogManager.LogTestOK($"Este es el tercer Step del caso de prueba");
                            LogManager.LogTestOK($"Se va a hacer una espera de 2s");
                            return ("End", 2f);
                        } 
                    },

                    { "End", () => 
                        {
                            LogManager.LogTestOK($"Ha terminado el caso de prueba: '{CurrentTestCase}'");
                            return ("", 0f);
                        } 
                    },
                };
            }

            protected void LoadRequiredData()
            {
                LogManager.LogTestOK($"> Cargando los datos para el test: '{this.Name}'");
            }

            protected void CreateTestCasesList()
            {
                LogManager.LogTestOK($"> Creando la lista de casos de prueba del test: '{this.Name}'");

                TestCasesList.Add(new(this, "TestCaseExample1"));
                TestCasesList.Add(new(this, "TestCaseExample2"));
            }

            protected void RecoverStatusFromPreviousExecution()
            {
                LogManager.LogTestOK($"> Recuperando el estado tras una posible ejecución anterior: '{this.Name}'");
                DeleteOldTestExecutionOutputFiles();
                CreateTestOutputDirectories();
            }

            protected void SetTestPreconditions()
            {
                LogManager.LogTestOK($"> Estableciendo las precondiciones del test: '{this.Name}'");
            }

            protected void OnTestStart()
            {
                LogManager.LogTestOK($"> Iniciando el test: '{this.Name}'");
            }

            protected void OnTestEnd()
            {
                if (HasErrors()) Status = TestStatus.Failed;
                else Status = TestStatus.Passed;

                LogManager.LogTestOK($"> Se han comprobado todos los casos de prueba del test: '{this.Name}'");

                // El log se copiará a la carpeta de Output del test al cerrar el programa, si hay log
                if (LogManager.ThisExecutionHasLogFileDump()) AppDomain.CurrentDomain.ProcessExit += (sender, args) => CopyLogToTestOutputFile();
            }

            private void DeleteOldTestExecutionOutputFiles()
            {
                string outputDirPath = TestManager.GetOutputRootPath() + "/" + Name;
                if (Directory.Exists(outputDirPath))
                {
                    LogManager.LogTestOK($"> Eliminando el directorio de /Ouput de una ejecucion anterior del test: '{this.Name}'");
                    Directory.Delete(outputDirPath, true);
                }
            }

            private void CreateTestOutputDirectories()
            {
                LogManager.LogTestOK($"> Creando los directorios necesarios para el test: '{this.Name}'");

                string outputRootPath = TestManager.GetOutputRootPath();

                Directory.CreateDirectory(outputRootPath);
                Directory.CreateDirectory(outputRootPath + "/" + Name);
            }

            private void CopyLogToTestOutputFile()
            {
                Thread.Sleep(500);
                File.Copy(LogManager.GetLogPath(), TestManager.GetOutputRootPath() + "/" + Name + "/" + "Log.html");
            }

            private void ExecutionLoop()
            {
                LogManager.LogTestOK($"> Van a comenzar a probarse todos los casos de prueba del test: '{this.Name}'");

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