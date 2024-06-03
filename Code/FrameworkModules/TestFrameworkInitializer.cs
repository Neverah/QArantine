using System;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;

using TestFramework.Code.FrameworkModules.GUI;

namespace TestFramework.Code.FrameworkModules
{
    public class TestFrameworkInitializer
    {
        private static bool _isTestFrameworkInitialized = false;

        private void CommonInit()
        {
            if (!IsTestFrameworkInitialized())
            {
                GUIManager.Instance.StartTestFrameworkGUI();
                _isTestFrameworkInitialized = true;
            }
            else
            {
                LogWarning("It seems that the TestFramework has been asked to initialize when it was already initialized, make sure that this is intentional. Generally, it is not intended to launch several tests in the same run.");
            }
        }

        public static bool IsTestFrameworkInitialized()
        {
            return _isTestFrameworkInitialized;
        }

        public void TestFrameworkInit(string? testClassToRun = null, string? testClassAssemblyName = null)
        {
            CommonInit();

            if (testClassToRun == null)
            {
                LogOK("No test run has been requested");
            }
            else
            {
                #pragma warning disable CS4014
                TestManager.Instance.LaunchTest(testClassToRun, testClassAssemblyName ?? GetCallingAssemblyName() ?? typeof(TestFramework.Code.FrameworkModules.TestManager).Assembly.GetName().Name ?? "TestFramework");
                #pragma warning restore CS4014
            }
        }

        public async Task TestFrameworkInitAndAwait(string? testClassToRun = null, string? testClassAssemblyName = null)
        {
            CommonInit();

            if (testClassToRun == null)
            {
                LogOK("No test run has been requested");
            }
            else
            {
                await TestManager.Instance.LaunchTest(testClassToRun, testClassAssemblyName ?? GetCallingAssemblyName() ?? typeof(TestFramework.Code.FrameworkModules.TestManager).Assembly.GetName().Name ?? "TestFramework");
            }
        }

        private static string? GetCallingAssemblyName()
        {
            // Get CallStack
            StackTrace stackTrace = new StackTrace();
            // Get the method who called this func (2 levels up)
            StackFrame? frame = stackTrace.GetFrame(2);

            if (frame != null)
            {
                // Get the calling method
                MethodBase? method = frame.GetMethod();
                if (method != null)
                {
                    // Get the calling method assembly
                    Assembly? callingAssembly = method.DeclaringType?.Assembly;
                    string? assemblyName = callingAssembly?.GetName().Name;
                    if(assemblyName != null && assemblyName != "System.Private.CoreLib") return assemblyName;
                    return null;
                }
            }
            return null;
        }
    }
}