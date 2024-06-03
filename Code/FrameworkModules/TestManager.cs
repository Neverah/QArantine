using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

using TestFramework.Code.Test;

namespace TestFramework.Code.FrameworkModules
{
    public sealed class TestManager
    {   
        public FrameworkTest? CurrentTest { get { return testLauncher.CurrentTest; } }
        private TestLauncher testLauncher;

        private static TestManager? _instance;
        private TestManager()
        {
            LogManager.TestManager = this;
            testLauncher = new TestLauncher();
        }

        public static TestManager Instance
        {
            get
            {
                _instance ??= new();
                return _instance;
            }
        }

        public async Task LaunchTest(string testClassName, string assemblyName)
        {
            await testLauncher.LaunchTest(testClassName, assemblyName);
        }

        public string GetOutputRootPath()
        {
             return testLauncher.GetOutputRootPath();
        }

        public string GetTestsNamespace()
        {
            return testLauncher.GetTestsNamespace();
        }
    }
}