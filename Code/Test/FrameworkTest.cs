namespace TestFramework.Code
{
    namespace Test
    {
        using TestFlowGraph = Dictionary<string, Func<(String, int)>>;
        public abstract class FrameworkTest
        {
            private string State { get; set; }
            private bool Paused { get; set; }
            private float Timeout { get; set; }

            protected TestFlowGraph flowGraph;

            public FrameworkTest()
            {
                State = "Executing";
                Paused = false;
                Timeout = 30f;

                flowGraph = CreateFlowGraph();
            }

            public void Launch()
            {
                Console.WriteLine($"Comenzando la ejecución del test: {this.GetType().Name}");

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

            protected TestFlowGraph CreateFlowGraph()
            {
                return new TestFlowGraph
                {
                    { "Init", () => 
                        {
                            Console.WriteLine($"Comenzando el flujo de ejecución del test: {this.GetType().Name}");
                            return ("End", 3);
                        } 
                    },

                    { "End", () => 
                        {
                            Console.WriteLine($"Fin del flujo de ejecución del test: {this.GetType().Name}");
                            return ("", 0);
                        } 
                    },
                };
            }
        }
    }
}