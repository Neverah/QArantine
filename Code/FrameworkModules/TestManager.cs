using System.Reflection;
using TestFramework.Code.Test;

namespace TestFramework.Code.FrameworkModules
{
    public class TestManager
    {
        public const string OUTPUT_DIRECTORY = "TestOutput";
        public FrameworkTest? CurrentTest { get; set; }

        private static TestManager? _instance;
        private TestManager() => LogManager.TestManager = this;
        public static TestManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new();
                }
                return _instance;
            }
        }

        public void LaunchTest(string testClassName)
        {
            CreateTestDirectories();
            LogManager.StartTestLogFile(testClassName);

            if (testClassName == null)
            {
                LogManager.LogError($"No se ha indicado la clase del test que se quiere ejecutar");
                return;
            }
            string fullTestClassName = "TestFramework.FrameworkTests." + testClassName;

            Type? testClass = Type.GetType(fullTestClassName);
            if (testClass == null)
            {
                LogManager.LogError($"No se ha encontrado un test con la clase indicada: {fullTestClassName}");
                return;
            }

            FrameworkTest? testInstance = (FrameworkTest?)Activator.CreateInstance(testClass);
            MethodInfo? testLaunchMethod = testClass.GetMethod("Launch");
            if (testLaunchMethod == null)
            {
                LogManager.LogError($"No se ha encontrado el método 'Launch' en la clase del test: {fullTestClassName}");
                return;
            }
            
            LogManager.LogOK($"Se va a ejecutar el test: {testClassName}");
            CurrentTest = testInstance;
            testLaunchMethod.Invoke(testInstance, null);
        }

        private static void CreateTestDirectories()
        {
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, OUTPUT_DIRECTORY));
        }
    }
}