namespace TestFramework.Code.Modules
{
    namespace TestLauncher
    {
        public class TestLauncher
        {
            public void LaunchTest(String testName)
            {
                Console.WriteLine($"Nombre del test a ejecutar: {testName}");

                // Crea un diccionario de funciones utilizando una clave de tipo string
                Dictionary<string, Func<int, int, int>> functionDictionary = new Dictionary<string, Func<int, int, int>>
                {
                    { "sumar", (a, b) => a + b },
                    { "restar", (a, b) => a - b }
                };

                // Llama a las funciones usando la clave
                int resultado1 = functionDictionary["sumar"](5, 3);
                int resultado2 = functionDictionary["restar"](10, 4);

                Console.WriteLine($"Resultado de sumar: {resultado1}");
                Console.WriteLine($"Resultado de restar: {resultado2}");
            }
        }
    }
}