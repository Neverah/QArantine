namespace TestFramework.Code
{
    namespace Test
    {
        using System.Text.Json;
        using TestFramework.Code.FrameworkModules;
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
            public DateTime TestStartTime { get; }
            public TestCase CurrentTestCase { get; set; }
            public string TestCaseInitStepID { get; set; }
            public string TestCaseEndStepID { get; set; }
            protected readonly List<TestCase> TestCasesList;

            private CancellationToken TimeoutToken;
            private readonly Dictionary<string, Func<(String, float)>> FlowChart;

            public FrameworkTest()
            {
                TestStartTime = DateTime.Now;

                Name = GetType().Name;

                TestCaseInitStepID = "Init";
                TestCaseEndStepID = "End";

                Paused = false;
                State = TestState.Testing;

                FlowChart = CreateFlowChart();
                TestCasesList = new();
                CurrentTestCase = new(this, "NO_TEST_CASE");
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

            public virtual TestError CreateTestError(string errorID, string errorCategory)
            {
                return new(Name, CurrentTestCase != null ? CurrentTestCase.ID : "UNKNOWN_TEST_CASE", CurrentTestCase != null ? CurrentTestCase.CurrentStep.ID : "UNKNOWN_TEST_STEP", errorID, errorCategory);
            }

            public virtual TestError CreateTestError(string errorID, string errorCategory, string errorDescription)
            {
                return new(Name, CurrentTestCase != null ? CurrentTestCase.ID : "UNKNOWN_TEST_CASE", CurrentTestCase != null ? CurrentTestCase.CurrentStep.ID : "UNKNOWN_TEST_STEP", errorID, errorCategory, errorDescription);
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

            protected virtual int GetFoundErrorsAmmount()
            {
                int countResult = 0;
                foreach(TestCase testCase in TestCasesList)
                {
                    countResult += testCase.TestCaseErrors.Count;
                }
                return countResult;
            }

            protected abstract Dictionary<string, Func<(String, float)>> CreateFlowChart();

            protected virtual void LoadRequiredData()
            {
                LogManager.LogTestOK($"> Loading the data for the test: '{Name}'");
            }

            protected virtual void CreateTestCasesList()
            {
                LogManager.LogTestOK($"> Creating the list of TestCases for the test: '{Name}'");
            }

            protected virtual void RecoverStateFromPreviousExecution()
            {
                LogManager.LogTestOK($"> Retrieving the state after a possible previous execution: '{Name}'");
                DeleteOldTestExecutionOutputFiles();
                CreateTestOutputDirectories();
            }

            protected virtual void SetTestPreconditions()
            {
                LogManager.LogTestOK($"> Setting the test's preconditions: '{Name}'");
            }

            protected virtual void OnTestStart()
            {
                LogManager.LogTestOK($"> Starting the test: '{Name}'");
            }

            protected virtual void OnTestEnd()
            {
                if (HasErrors()) State = TestState.Failed;
                else State = TestState.Passed;

                LogManager.LogTestOK($"> The test has finished: '{Name}'");

                LogReportOnEnd();
                PrintTestResultJSONFile();
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

            protected virtual void DeleteOldTestExecutionOutputFiles()
            {
                string outputDirPath = TestManager.GetOutputRootPath() + "/" + Name;
                if (Directory.Exists(outputDirPath))
                {
                    LogManager.LogTestOK($"> Deleting the /Output directory from a previous test run: '{Name}'");
                    Directory.Delete(outputDirPath, true);
                }
            }

            protected virtual void CreateTestOutputDirectories()
            {
                LogManager.LogTestOK($"> Creating the necessary directories for the test: '{Name}'");

                string outputRootPath = TestManager.GetOutputRootPath();

                Directory.CreateDirectory(outputRootPath);
                Directory.CreateDirectory(outputRootPath + "/" + Name);
                Directory.CreateDirectory(outputRootPath + "/" + Name + "/Coverage");
            }

            protected virtual void PrintTestResultJSONFile()
            {
                if (!ConfigManager.GetTFConfigParamAsBool("DumpTestResultToJSONFile")) return;

                Dictionary<string, object> testResult = new()
                {
                    { "Success", !HasErrors() },
                    { "TestedTestCases", TestCasesList.Count },
                    { "FoundErrorsAmmount", GetFoundErrorsAmmount() },
                    { "CompletionTime", TimeManager.GetAppElapsedTimeAsFormatedString() },
                    { "CompletionTimeAsSeconds", TimeManager.GetAppElapsedSecondsAsLong() },
                    { "StartTimestamp", TestStartTime.ToUniversalTime().ToString("o") },
                    { "EndTimestamp", DateTime.Now.ToUniversalTime().ToString("o") },
                };

                using (StreamWriter errorsWriter = new(TestManager.GetOutputRootPath() + "/" + Name + "/TestResult.json"))
                {
                    errorsWriter.Write(JsonSerializer.Serialize(testResult, new JsonSerializerOptions { WriteIndented = ConfigManager.GetTFConfigParamAsBool("IndentReportSystemJsonFiles") }));
                }
            }

            protected virtual void PrintCoverageToJSONFile()
            {
                if (!ConfigManager.GetTFConfigParamAsBool("DumpCoverageToJSONFile")) return;

                List<string> testedCasesList = new();
                foreach(TestCase testCase in TestCasesList)
                {
                    testedCasesList.Add(testCase.ID);
                }

                using (StreamWriter errorsWriter = new(TestManager.GetOutputRootPath() + "/" + Name + "/Coverage/TestedTestCases.json"))
                {
                    errorsWriter.Write(JsonSerializer.Serialize(testedCasesList, new JsonSerializerOptions { WriteIndented = ConfigManager.GetTFConfigParamAsBool("IndentReportSystemJsonFiles") }));
                }
            }

            protected virtual void PrintErrorsToJSONFile()
            {
                if (!ConfigManager.GetTFConfigParamAsBool("DumpErrorsToJSONFile")) return;

                List<TestError> errorsList = new();
                foreach(TestCase testCase in TestCasesList)
                {
                    errorsList.AddRange(testCase.TestCaseErrors);
                }

                using (StreamWriter errorsWriter = new(TestManager.GetOutputRootPath() + "/" + Name + "/TestFoundErrors.json"))
                {
                    errorsWriter.Write(JsonSerializer.Serialize(errorsList, new JsonSerializerOptions { WriteIndented = ConfigManager.GetTFConfigParamAsBool("IndentReportSystemJsonFiles") }));
                }
            }

            protected virtual void CopyLogsToTestOutputFile()
            {
                Thread.Sleep(500);
                if (File.Exists(LogManager.GetLogPath())) File.Copy(LogManager.GetLogPath(), TestManager.GetOutputRootPath() + "/" + Name + "/Log.html");
                if (LogManager.ThisExecutionHasErrorLogFileDump() && File.Exists(LogManager.GetErrorsLogPath())) File.Copy(LogManager.GetErrorsLogPath(), TestManager.GetOutputRootPath() + "/" + Name + "/ErrorsLog.html");
            }


            private void ExecutionLoop()
            {
                LogManager.LogTestOK($"> Testing of all TestCases in this test is about to begin: '{Name}'");

                CurrentTestCase = GetNextTestCase()!;
                CurrentTestCase?.CurrentStep.OnTestStepStart();
                while (CurrentTestCase != null)
                {
                    // If a timeout has been requested, the test will fail, stop, and an attempt will be made to perform a controlled shutdown
                    if (CheckForcedCloseTokenSignal()) break;

                    if (CurrentTestCase.State == TestCase.TestCaseState.Testing)
                    {
                        string previousStepID = CurrentTestCase.CurrentStep.ID;

                        (string nextStep, float delay) = FlowChart[previousStepID]();
                        CurrentTestCase.UpdateTestStep(nextStep);

                        if (delay >= 0) Thread.Sleep((int)delay * 1000);
                    }
                    else
                    {
                        CurrentTestCase = GetNextTestCase()!;
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
                    ReportTestError(CreateTestError("TEST_TIMEOUT", "TEST_ISSUE")
                        .AddExtraField("TestExecutionTime", TimeManager.GetAppElapsedSecondsAsString())
                    );
                }
                return TimeoutToken.IsCancellationRequested;
            }
        }
    }
}