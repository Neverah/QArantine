using TestFramework.Code.FrameworkModules;
class LaunchTestExample
{
    static async Task Main(string[] args)
    {
        // Verificar si se proporcionó al menos un argumento
        if (args.Length <= 0)
        {
            LogManager.LogFatalError("Please provide the name of the test class you want to execute as an input parameter");
            Environment.Exit(-1);
        }
        
        await TestManager.Instance.LaunchTest(args[0]);
    }
}