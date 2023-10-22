using System.Reflection;
using TestFramework.Code.Test;

namespace TestFramework.Code.FrameworkModules
{
    public sealed class TestManager
    {
        public FrameworkTest? CurrentTest { get; set; }

        private static string? OutputRootPath;
        private static string? TestsNamespace;
        private CancellationTokenSource? TestCancellationTokenSource;
        private Thread? CurrentTestThread;

        private static TestManager? _instance;
        private TestManager()
        {
            LogManager.TestManager = this;
            OutputRootPath = "";
            TestsNamespace = "";
        }

        public static TestManager Instance
        {
            get
            {
                _instance ??= new();
                return _instance;
            }
        }

        public async Task LaunchTest(string testClassName)
        {
            Init();

            Type? testClass = GetTestClass(testClassName);
            if (testClass == null) Environment.Exit(-1);

            CurrentTest = GetTestInstance(testClass);
            if (CurrentTest == null) Environment.Exit(-1);

            MethodInfo? testLaunchMethod = GetTestLaunchMethod(testClass);
            if (testLaunchMethod == null) Environment.Exit(-1);
            
            CreateTestThread(testLaunchMethod);
            await CreateTestTimeoutAwaitThreadAsync();
        }

        public static string GetOutputRootPath()
        {
            if (OutputRootPath != null && OutputRootPath != "") return OutputRootPath;

            if ((OutputRootPath = ConfigManager.GetTFConfigParamAsString("TestFrameworkOutputRootPath")!) == null)
            {
                LogManager.LogFatalError("Could not find the 'TestFrameworkOutputRootPath' config param, the test can not continue, aborting");
                Environment.Exit(-1);
            }
            
            if (!Path.IsPathRooted(OutputRootPath)) OutputRootPath = Path.Combine(Environment.CurrentDirectory, OutputRootPath);

            return OutputRootPath;
        }

        public static string GetTestsNamespace()
        {
            if (TestsNamespace != null && TestsNamespace != "") return TestsNamespace;

            if ((TestsNamespace = ConfigManager.GetTFConfigParamAsString("MainTestsNamespace")!) == null)
            {
                LogManager.LogFatalError("Could not find the 'MainTestsNamespace' config param, the test can not continue, aborting");
                Environment.Exit(-1);
            }

            return TestsNamespace;
        }

        private static void Init()
        {
            LogManager.StartLogFile();
        }

        private static Type? GetTestClass(string testClassName)
        {
            if (testClassName == null)
            {
                LogManager.LogFatalError($"The test class to be executed has not been specified");
                return null;
            }
            string fullTestClassName = GetTestsNamespace() + "." + testClassName;

            Type? testClass = Type.GetType(fullTestClassName);
            if (testClass == null)
            {
                LogManager.LogFatalError($"No test with the specified class has been found: {fullTestClassName}");
                return null;
            }

            return testClass;
        }

        private static FrameworkTest? GetTestInstance(Type testClass)
        {
            FrameworkTest? testInstance = (FrameworkTest?)Activator.CreateInstance(testClass);
            if (testInstance == null)
            {
                LogManager.LogFatalError($"An instance of the test class could not be created: {testClass.Name}");
                return null;
            }
            return testInstance;
        }

        private static MethodInfo? GetTestLaunchMethod(Type testClass)
        {
            MethodInfo? testLaunchMethod = testClass.GetMethod("Launch");
            if (testLaunchMethod == null)
            {
                LogManager.LogFatalError($"The 'Launch' method was not found in the test class: {testClass.Name}");
                return null;
            }
            return testLaunchMethod;
        }

        private void CreateTestThread(MethodInfo testLaunchMethod)
        {
            LogManager.LogOK($"A new thread will be created to execute the test: {CurrentTest?.Name}");

            TestCancellationTokenSource = new();

            CurrentTestThread = new(() =>
            {
                object[] arguments = new object[] {TestCancellationTokenSource.Token};
                testLaunchMethod.Invoke(CurrentTest, arguments);
            });
            CurrentTestThread.Start();
        }

        private async Task CreateTestTimeoutAwaitThreadAsync()
        {
            LogManager.LogOK($"A new thread will be created to handle the test timeout: {CurrentTest?.Name}");
            await WaitForCurrentTest(CurrentTest?.Timeout ?? 0);
        }

        private async Task WaitForCurrentTest(float timeout)
        {
            float elapsedSecs = 0;
            while(CurrentTestThread?.IsAlive ?? false)
            {
                if (elapsedSecs < timeout)
                {
                    elapsedSecs++;
                    await Task.Delay(1000);
                }
                else
                {
                    LogManager.LogError($"The test '{CurrentTest?.Name}' has timed out after {CurrentTest?.Timeout} seconds and will be stopped. If it doesn't stop within 10 seconds, it will be forcibly terminated");
                    // Primero, se envía un aviso para que el test se cierre de forma legal
                    TestCancellationTokenSource?.Cancel();
                    // Si el test no se ha cerrado tras 10s, se fuerza el fin de su Thread
                    if(!(CurrentTestThread?.Join(10000) ?? true))
                    {
                        LogManager.LogFatalError($"The test '{CurrentTest?.Name}' did not close properly after 10 seconds of sending the signal. The program will be closed");
                        Environment.Exit(-2);
                    }
                    return;
                }
            }
            LogManager.LogOK($"The test '{CurrentTest?.Name}' has completed successfully within its maximum time frame, the execution has finished");
            Environment.Exit(0);
        }
    }
}