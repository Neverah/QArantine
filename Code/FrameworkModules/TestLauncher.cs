using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

using TestFramework.Code.Test;

namespace TestFramework.Code.FrameworkModules
{
    public sealed class TestLauncher
    {
        public FrameworkTest? CurrentTest { get; private set; }

        private string? OutputRootPath;
        private string? TestsNamespace;
        private CancellationTokenSource? TestCancellationTokenSource;
        private Thread? CurrentTestThread;

        public TestLauncher()
        {
            OutputRootPath = "";
            TestsNamespace = "";
        }

        public async Task LaunchTest(string testClassName, string assemblyName)
        {
            Type? testClass = GetTestClass(testClassName, assemblyName);
            if (testClass == null) Environment.Exit(-1);

            CurrentTest = GetTestInstance(testClass);
            if (CurrentTest == null) Environment.Exit(-1);

            MethodInfo? testLaunchMethod = GetTestLaunchMethod(testClass);
            if (testLaunchMethod == null) Environment.Exit(-1);
            
            CreateTestThread(testLaunchMethod);
            await CreateTestTimeoutAwaitThreadAsync();
        }

        public string GetOutputRootPath()
        {
            if (OutputRootPath != null && OutputRootPath != "") return OutputRootPath;

            if ((OutputRootPath = ConfigManager.GetTFConfigParamAsString("TestFrameworkOutputRootPath")!) == null)
            {
                LogFatalError("Could not find the 'TestFrameworkOutputRootPath' config param, the test can not continue, aborting");
                Environment.Exit(-1);
            }
            
            if (!Path.IsPathRooted(OutputRootPath)) OutputRootPath = Path.Combine(Environment.CurrentDirectory, OutputRootPath);

            return OutputRootPath;
        }

        public string GetTestsNamespace()
        {
            if (TestsNamespace != null && TestsNamespace != "") return TestsNamespace;

            if ((TestsNamespace = ConfigManager.GetTFConfigParamAsString("MainTestsNamespace")!) == null)
            {
                LogFatalError("Could not find the 'MainTestsNamespace' config param, the test can not continue, aborting");
                Environment.Exit(-1);
            }

            return TestsNamespace;
        }

        private Type? GetTestClass(string testClassName, string assemblyName)
        {
            if (testClassName == null)
            {
                LogFatalError($"The test class to be executed has not been specified");
                return null;
            }

            Assembly? testAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
            if (testAssembly == null)
            {
                testAssembly = GetTestAssembly(assemblyName);
                if (testAssembly == null) return null;
            }

            string fullTestClassName = GetTestsNamespace() + "." + testClassName;

            Type? testClass = testAssembly?.GetType(fullTestClassName);
            if (testClass == null)
            {
                LogFatalError($"No test with the specified class has been found: {fullTestClassName}");
                return null;
            }

            return testClass;
        }

        private Assembly? GetTestAssembly(string assemblyName)
        {
            Assembly? testAssembly = null;
            try
            {
                testAssembly = Assembly.Load(assemblyName);
            }
            catch (Exception ex)
            {
                LogFatalError($"Failed to load test assembly with name '{assemblyName}': {ex.Message}");
                return null;
            }
            return testAssembly;
        }

        private FrameworkTest? GetTestInstance(Type testClass)
        {
            FrameworkTest? testInstance = (FrameworkTest?)Activator.CreateInstance(testClass);
            if (testInstance == null)
            {
                LogFatalError($"An instance of the test class could not be created: {testClass.Name}");
                return null;
            }
            return testInstance;
        }

        private MethodInfo? GetTestLaunchMethod(Type testClass)
        {
            MethodInfo? testLaunchMethod = testClass.GetMethod("Launch");
            if (testLaunchMethod == null)
            {
                LogFatalError($"The 'Launch' method was not found in the test class: {testClass.Name}");
                return null;
            }
            return testLaunchMethod;
        }

        private void CreateTestThread(MethodInfo testLaunchMethod)
        {
            LogOK($"A new thread will be created to execute the test: {CurrentTest?.Name}");

            TestCancellationTokenSource = new();

            CurrentTestThread = new(() =>
            {
                object[] arguments = new object[] {TestCancellationTokenSource.Token};
                testLaunchMethod.Invoke(CurrentTest, arguments);
            });
            CurrentTestThread.IsBackground = true;
            CurrentTestThread.Start();
        }

        private async Task CreateTestTimeoutAwaitThreadAsync()
        {
            LogOK($"A new thread will be created to handle the test timeout: {CurrentTest?.Name}");
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
                    LogError($"The test '{CurrentTest?.Name}' has timed out after {CurrentTest?.Timeout} seconds and will be stopped. If it doesn't stop within 10 seconds, it will be forcibly terminated");
                    // Primero, se envía un aviso para que el test se cierre de forma legal
                    TestCancellationTokenSource?.Cancel();
                    // Si el test no se ha cerrado tras 10s, se fuerza el fin de su Thread
                    if(!(CurrentTestThread?.Join(10000) ?? true))
                    {
                        LogFatalError($"The test '{CurrentTest?.Name}' did not close properly after 10 seconds of sending the signal. The program will be closed");
                        HandleTestClosureDependecies();
                        await Task.Delay(1000);
                        Environment.Exit(-2);
                    }
                    return;
                }
            }
            LogOK($"The test '{CurrentTest?.Name}' has completed successfully within its maximum time frame, the execution has finished");
            await Task.Delay(1000);
            Environment.Exit(0);
        }

        private void HandleTestClosureDependecies()
        {
            if(ConfigManager.GetTFConfigParamAsBool("RecordTestExecutionVideo"))
                RecordingManager.Instance.StopRecording();
        }
    }
}