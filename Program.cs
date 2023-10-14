using TestFramework.Code.FrameworkModules;
class Program
{
    static void Main(string[] args)
    {
        // Verificar si se proporcionó al menos un argumento
        if (args.Length <= 0)
        {
            LogManager.LogError("Por favor, proporciona el nombre de la clase del test que quieres ejecutar como parámetro de entrada.");
            return;
        }
        
        TestManager.Instance.LaunchTest(args[0]);
    }
}