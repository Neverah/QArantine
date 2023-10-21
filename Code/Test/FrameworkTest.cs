namespace TestFramework.Code
{
    namespace Test
    {
        using System.Text.Json;
        using System.Text.Json.Serialization;
        using TestFramework.Code.FrameworkModules;
        using TestFlowChart = Dictionary<string, Func<(String, float)>>;
        public abstract class FrameworkTest
        {
            public enum TestState
            {
                Testing,
                Passed,
                Failed
            }

            public float Timeout = 60f;
            public bool Paused { get; set; }
            public TestState State { get; set; }
            public string Name { get; }
            public TestCase? CurrentTestCase { get; set; }
            public string TestCaseInitStepID { get; set; }
            public string TestCaseEndStepID { get; set; }

            private CancellationToken TimeoutToken;
            private readonly TestFlowChart FlowChart;
            private readonly List<TestCase> TestCasesList;

            public FrameworkTest()
            {
                Name = GetType().Name;

                TestCaseInitStepID = "Init";
                TestCaseEndStepID = "End";

                Paused = false;
                State = TestState.Testing;

                FlowChart = CreateFlowChart();
                TestCasesList = new();
            }

            public virtual void Launch(CancellationToken timeoutToken)
            {
                TimeoutToken = timeoutToken;

                LoadRequiredData();
                CreateTestCasesList();
                RecoverStateFromPreviousExecution();
                SetTestPreconditions();
                OnTestStart();
                ExecutionLoop();
            }

            public virtual TestError CreateTestError(string errorID)
            {
                return new(Name, CurrentTestCase != null ? CurrentTestCase.ID : "UNKNOWN_TEST_CASE", CurrentTestCase != null ? CurrentTestCase.CurrentStep : "UNKNOWN_TEST_STEP", errorID);
            }

            public virtual void ReportTestError(TestError newError)
            {
                LogManager.LogTestError($"TEST ERROR DETECTED > {newError}");
                CurrentTestCase?.AddError(newError);
            }

            protected virtual bool HasErrors()
            {
                foreach(TestCase testCase in TestCasesList)
                {
                    if (testCase.HasErrors()) return true;
                }
                return false;
            }

            protected virtual TestFlowChart CreateFlowChart()
            {
                return new TestFlowChart
                {
                    { "Init", () => 
                        {
                            LogManager.LogTestOK($"Starting the current TestCase: '{CurrentTestCase}'");
                            return ("Introduction", 0f);
                        } 
                    },

                    { "Introduction", () => 
                        {
                            LogManager.LogTestOK($"This is the first TestStep of the current TestCase");
                            LogManager.LogTestOK($"You should try implementing your own test by creating a flowchart that meets your needs");
                            return ("ErrorReportExample", 0f);
                        } 
                    },

                    { "ErrorReportExample", () => 
                        {
                            LogManager.LogTestOK($"This is the second step of the current TestCase");
                            LogManager.LogTestOK($"An example of a simple error report (without additional fields):");
                            ReportTestError(CreateTestError("EXAMPLE_OF_SIMPLE_ERROR"));
                            LogManager.LogTestOK($"An example of an extended error report (with additional fields):");
                            ReportTestError(CreateTestError("EXAMPLE_OF_EXTENDED_ERROR")
                                .AddExtraField("Today Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                                .AddExtraField("1 + 1", 1 + 1)
                                .AddExtraField("Value Of PI", Math.PI)
                                .AddExtraField("Roses are", "Blue")
                            );
                            LogManager.LogTestOK($"Two errors in the same TestCase with all their fields being the same (excluding the TestStep) are considered equal, and duplicates are discarded");
                            ReportTestError(CreateTestError("EXAMPLE_OF_EXTENDED_ERROR")
                                .AddExtraField("Today Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                                .AddExtraField("1 + 1", 1 + 1)
                                .AddExtraField("Value Of PI", Math.PI)
                                .AddExtraField("Roses are", "Blue")
                            );
                            return ("WaitExample", 0f);
                        } 
                    },

                    { "WaitExample", () => 
                        {
                            LogManager.LogTestOK($"This is the third step of the current TestCase");
                            LogManager.LogTestOK($"A 2-second delay will be executed");
                            return ("End", 2f);
                        } 
                    },

                    { "End", () => 
                        {
                            LogManager.LogTestOK($"The TestCase has been completed: '{CurrentTestCase}'");
                            return ("", 0f);
                        } 
                    },
                };
            }

            protected virtual void LoadRequiredData()
            {
                LogManager.LogTestOK($"> Loading the data for the test: '{this.Name}'");
            }

            protected virtual void CreateTestCasesList()
            {
                LogManager.LogTestOK($"> Creating the list of TestCases for the test: '{this.Name}'");

                TestCasesList.Add(new(this, "TestCaseExample1"));
                TestCasesList.Add(new(this, "TestCaseExample2"));
            }

            protected virtual void RecoverStateFromPreviousExecution()
            {
                LogManager.LogTestOK($"> Retrieving the state after a possible previous execution: '{this.Name}'");
                DeleteOldTestExecutionOutputFiles();
                CreateTestOutputDirectories();
            }

            protected virtual void SetTestPreconditions()
            {
                LogManager.LogTestOK($"> Setting the test's preconditions: '{this.Name}'");
            }

            protected virtual void OnTestStart()
            {
                LogManager.LogTestOK($"> Starting the test: '{this.Name}'");
            }

            protected virtual void OnTestEnd()
            {
                if (HasErrors()) State = TestState.Failed;
                else State = TestState.Passed;

                LogManager.LogTestOK($"> The test has finished: '{this.Name}'");

                LogReportOnEnd();
                PrintCoverageToJSONFile();
                PrintErrorsToJSONFile();

                // El log se copiará a la carpeta de Output del test al cerrar el programa, si hay log
                if (LogManager.ThisExecutionHasLogFileDump()) AppDomain.CurrentDomain.ProcessExit += (sender, args) => CopyLogsToTestOutputFile();
            }

            protected virtual void LogReportOnEnd()
            {
                LogCoverageOnEnd();
                LogErrorsOnEnd();
            }

            protected virtual void LogCoverageOnEnd()
            {
                LogManager.LogOK("\n>> Test coverage section:\n");
                LogManager.LogOK("> TestCases:");
                foreach(TestCase testCase in TestCasesList)
                {
                    LogManager.LogOK("- " + testCase.ID);
                }
            }

            protected virtual void LogErrorsOnEnd()
            {
                LogManager.LogOK("\n>> Test errors section:\n");
                foreach(TestCase testCase in TestCasesList)
                {
                    if (testCase.HasErrors())
                    {
                        LogManager.LogOK("> " + testCase.ID + ":");
                        testCase.LogFoundErrors();
                        LogManager.LogOK("");
                    }
                }
            }

            private void DeleteOldTestExecutionOutputFiles()
            {
                string outputDirPath = TestManager.GetOutputRootPath() + "/" + Name;
                if (Directory.Exists(outputDirPath))
                {
                    LogManager.LogTestOK($"> Deleting the /Output directory from a previous test run: '{Name}'");
                    Directory.Delete(outputDirPath, true);
                }
            }

            private void CreateTestOutputDirectories()
            {
                LogManager.LogTestOK($"> Creating the necessary directories for the test: '{Name}'");

                string outputRootPath = TestManager.GetOutputRootPath();

                Directory.CreateDirectory(outputRootPath);
                Directory.CreateDirectory(outputRootPath + "/" + Name);
                Directory.CreateDirectory(outputRootPath + "/" + Name + "/Coverage");
            }

            private void CopyLogsToTestOutputFile()
            {
                Thread.Sleep(500);
                if (File.Exists(LogManager.GetLogPath())) File.Copy(LogManager.GetLogPath(), TestManager.GetOutputRootPath() + "/" + Name + "/Log.html");
                if (LogManager.ThisExecutionHasErrorLogFileDump() && File.Exists(LogManager.GetErrorsLogPath())) File.Copy(LogManager.GetErrorsLogPath(), TestManager.GetOutputRootPath() + "/" + Name + "/ErrorsLog.html");
            }

            private void PrintCoverageToJSONFile()
            {
                if (!ConfigManager.GetTFConfigParamAsBool("DumpCoverageToJSONFile")) return;

                List<string> testedCasesList = new();
                foreach(TestCase testCase in TestCasesList)
                {
                    testedCasesList.Add(testCase.ID);
                }

                using (StreamWriter errorsWriter = new(TestManager.GetOutputRootPath() + "/" + Name + "/Coverage/TestedTestCases.json"))
                {
                    errorsWriter.Write(JsonSerializer.Serialize(testedCasesList, new JsonSerializerOptions { WriteIndented = TestManager.ShouldIndentReportSystemJsonFiles() }));
                }
            }

            private void PrintErrorsToJSONFile()
            {
                if (!ConfigManager.GetTFConfigParamAsBool("DumpErrorsToJSONFile")) return;

                List<TestError> errorsList = new();
                foreach(TestCase testCase in TestCasesList)
                {
                    errorsList.AddRange(testCase.TestCaseErrors);
                }

                using (StreamWriter errorsWriter = new(TestManager.GetOutputRootPath() + "/" + Name + "/TestFoundErrors.json"))
                {
                    errorsWriter.Write(JsonSerializer.Serialize(errorsList, new JsonSerializerOptions { WriteIndented = TestManager.ShouldIndentReportSystemJsonFiles() }));
                }
            }

            private void ExecutionLoop()
            {
                LogManager.LogTestOK($"> Testing of all TestCases in this test is about to begin: '{this.Name}'");

                CurrentTestCase = GetNextTestCase();
                while (CurrentTestCase != null)
                {
                    // If a timeout has been requested, the test will fail, stop, and an attempt will be made to perform a controlled shutdown
                    if (CheckForcedCloseTokenSignal()) break;

                    if (CurrentTestCase.State == TestCase.TestCaseState.Testing)
                    {
                        string previousStep = CurrentTestCase.CurrentStep;

                        (string nextStep, float delay) = FlowChart[previousStep]();
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
                    if (testCase.State == TestCase.TestCaseState.Testing)
                    {
                        return testCase;
                    }
                }
                return null;
            }

            private bool CheckForcedCloseTokenSignal()
            {
                if (TimeoutToken.IsCancellationRequested)
                {
                    LogManager.LogTestError($"The test '{Name}' has reached the timeout, and its cancellation has been requested. An attempt will be made to perform a controlled shutdown");
                    ReportTestError(CreateTestError("TEST_TIMEOUT")
                        .AddExtraField("TestExecutionTime", TimeManager.GetAppElapsedTimeAsString())
                    );
                }
                return TimeoutToken.IsCancellationRequested;
            }
        }
    }
}