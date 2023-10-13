using TestFramework.Code.Modules.TestLauncher;
class Program
{
    static void Main(string[] args)
    {
        // Verificar si se proporcionó al menos un argumento
        if (args.Length > 0)
        {
            TestLauncher testLauncher = new();
            testLauncher.LaunchTest(args[0]);
        }
        else
        {
            Console.WriteLine("Por favor, proporciona un nombre de test como argumento de consola.");
        }
    }
}