namespace QArantine.Code.Test
{
    using System.Text.Json;
    using QArantine.Code.FrameworkModules;
    public abstract class QArantineTest
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

        private JsonSerializerOptions outputSerializer = new(){ WriteIndented = ConfigManager.GetTFConfigParamAsBool("IndentReportSystemJsonFiles")};
        private CancellationToken TimeoutToken;
        private readonly Dictionary<string, Func<(String, float)>> FlowChart;

        public QArantineTest()
        {
            TestStartTime = DateTime.Now;

            Name = GetType().Name;

            TestCaseInitStepID = "Init";
            TestCaseEndStepID = "End";

            Paused = false;
            State = TestState.Testing;

            FlowChart = CreateFlowChart();
            TestCasesList = [];
            CurrentTestCase = new(this, "NO_TEST_CASE");
        }

        public virtual void Launch(CancellationToken timeoutToken)
        {
            TimeoutToken = timeoutToken;

            LoadRequiredData();
            CreateTestCasesList();
            RecoverStateFromPreviousExecution();
            SetTestPreconditions();
            PreLaunchChecks();
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
            LogTestError($"TEST ERROR DETECTED > {newError}");
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
            LogTestOK($"> Loading the data for the test: '{Name}'");
        }

        protected virtual void CreateTestCasesList()
        {
            LogTestOK($"> Creating the list of TestCases for the test: '{Name}'");
        }

        protected virtual void RecoverStateFromPreviousExecution()
        {
            LogTestOK($"> Retrieving the state after a possible previous execution: '{Name}'");
            DeleteOldTestExecutionOutputFiles();
            CreateTestOutputDirectories();
        }

        protected virtual void SetTestPreconditions()
        {
            LogTestOK($"> Setting the test's preconditions: '{Name}'");
        }

        protected virtual void PreLaunchChecks()
        {
            LogTestOK($"> Checking everything is ready for starting the test: '{Name}'");
        }

        protected virtual void OnTestStart()
        {
            LogTestOK($"> Starting the test: '{Name}'");

            if(ConfigManager.GetTFConfigParamAsBool("RecordTestExecutionVideo"))
                RecordingManager.Instance.StartRecordingWindow(TestManager.Instance.GetOutputRootPath() + "/" + Name + "/Video/" + Name + "Exec.mp4", ConfigManager.GetTFConfigParamAsString("WindowToRecordName")!);
        }

        protected virtual void OnTestEnd()
        {
            if (HasErrors()) State = TestState.Failed;
            else State = TestState.Passed;

            LogTestOK($"> The test has finished: '{Name}'");

            LogReportOnEnd();
            PrintTestResultJSONFile();
            PrintCoverageToJSONFile();
            PrintErrorsToJSONFile();

            if(ConfigManager.GetTFConfigParamAsBool("RecordTestExecutionVideo"))
                RecordingManager.Instance.StopRecording();

            // El log se copiará a la carpeta de Output del test al cerrar el programa, si hay log
            if (LogManager.fileLogHandler.ThisExecutionHasLogFileDump()) AppDomain.CurrentDomain.ProcessExit += (sender, args) => CopyLogsToTestOutputFile();
        }

        protected virtual void LogReportOnEnd()
        {
            LogCoverageOnEnd();
            LogErrorsOnEnd();
        }

        protected virtual void LogCoverageOnEnd()
        {
            LogOK("\n>> Test coverage section:\n");
            LogOK("> TestCases:");
            foreach(TestCase testCase in TestCasesList)
            {
                LogOK("- " + testCase.ID);
            }
        }

        protected virtual void LogErrorsOnEnd()
        {
            LogOK("\n>> Test errors section:\n");
            foreach(TestCase testCase in TestCasesList)
            {
                if (testCase.HasErrors())
                {
                    LogOK("> " + testCase.ID + ":");
                    testCase.LogFoundErrors();
                    LogOK("");
                }
            }
        }

        protected virtual void DeleteOldTestExecutionOutputFiles()
        {
            string outputDirPath = TestManager.Instance.GetOutputRootPath() + "/" + Name;
            if (Directory.Exists(outputDirPath))
            {
                LogTestOK($"> Deleting the /Output directory from a previous test run: '{Name}'");
                Directory.Delete(outputDirPath, true);
            }
        }

        protected virtual void CreateTestOutputDirectories()
        {
            LogTestOK($"> Creating the necessary directories for the test: '{Name}'");

            string outputRootPath = TestManager.Instance.GetOutputRootPath();

            Directory.CreateDirectory(outputRootPath);
            Directory.CreateDirectory(outputRootPath + "/" + Name);
            Directory.CreateDirectory(outputRootPath + "/" + Name + "/Video");
            Directory.CreateDirectory(outputRootPath + "/" + Name + "/Coverage");
        }

        protected virtual void PrintTestResultJSONFile()
        {
            if (!ConfigManager.GetTFConfigParamAsBool("DumpTestResultToJSONFile")) return;

            TestResult testResult = new(
                Name,
                !HasErrors(),
                TestCasesList.Count,
                GetFoundErrorsAmmount(),
                TimeManager.GetAppElapsedTimeAsFormatedString(),
                TimeManager.GetAppElapsedSecondsAsLong(),
                TestStartTime.ToUniversalTime().ToString("o"),
                DateTime.Now.ToUniversalTime().ToString("o"),
                Path.Combine(TestManager.Instance.GetOutputRootPath(), Name)
            );

            using StreamWriter errorsWriter = new(TestManager.Instance.GetOutputRootPath() + "/" + Name + "/TestResult.json");
            errorsWriter.Write(JsonSerializer.Serialize(testResult, outputSerializer));
        }

        protected virtual void PrintCoverageToJSONFile()
        {
            if (!ConfigManager.GetTFConfigParamAsBool("DumpCoverageToJSONFile")) return;

            List<string> testedCasesList = [];
            foreach(TestCase testCase in TestCasesList)
            {
                testedCasesList.Add(testCase.ID);
            }

            using (StreamWriter errorsWriter = new(TestManager.Instance.GetOutputRootPath() + "/" + Name + "/Coverage/TestedTestCases.json"))
            {
                errorsWriter.Write(JsonSerializer.Serialize(testedCasesList, outputSerializer));
            }
        }

        protected virtual void PrintErrorsToJSONFile()
        {
            if (!ConfigManager.GetTFConfigParamAsBool("DumpErrorsToJSONFile")) return;

            List<TestError> errorsList = [];
            foreach(TestCase testCase in TestCasesList)
            {
                errorsList.AddRange(testCase.TestCaseErrors);
            }

            using (StreamWriter errorsWriter = new(TestManager.Instance.GetOutputRootPath() + "/" + Name + "/TestFoundErrors.json"))
            {
                errorsWriter.Write(JsonSerializer.Serialize(errorsList, outputSerializer));
            }
        }

        protected virtual void CopyLogsToTestOutputFile()
        {
            Thread.Sleep(500);
            if (File.Exists(LogManager.fileLogHandler.GetLogPath())) File.Copy(LogManager.fileLogHandler.GetLogPath(), TestManager.Instance.GetOutputRootPath() + "/" + Name + "/Log.html");
            if (LogManager.fileLogHandler.ThisExecutionHasErrorLogFileDump() && File.Exists(LogManager.fileLogHandler.GetErrorsLogPath())) File.Copy(LogManager.fileLogHandler.GetErrorsLogPath(), TestManager.Instance.GetOutputRootPath() + "/" + Name + "/ErrorsLog.html");
        }


        private void ExecutionLoop()
        {
            LogTestOK($"> Testing of all TestCases in this test is about to begin: '{Name}'");

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
                LogTestError($"The test '{Name}' has reached the timeout, and its cancellation has been requested. An attempt will be made to perform a controlled shutdown");
                ReportTestError(CreateTestError("TEST_TIMEOUT", "TEST_ISSUE")
                    .AddExtraField("TestExecutionTime", TimeManager.GetAppElapsedSecondsAsString())
                );
            }
            return TimeoutToken.IsCancellationRequested;
        }
    }
}