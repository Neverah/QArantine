namespace TestFramework.Code.FrameworkModules
{
    public static class ConfigManager
    {
        public const string MAIN_CONFIG_FILE_PATH = "TestFramework.config";
        private static readonly Dictionary<string, string> ConfigParams;
        private static bool IsMainConfigAlreadyLoaded = false;

        static ConfigManager()
        {
            ConfigParams = new();
        }

        public static void LoadTestFrameworkMainConfig()
        {
            if (IsMainConfigAlreadyLoaded) return;

            ReadTestFrameworkMainConfigFile();
            IsMainConfigAlreadyLoaded = true;
            LogManager.LogOK("TestFramework Main Config loaded");
        }

        public static string? GetConfigParam(string paramID)
        {
            if (ConfigParams.TryGetValue(paramID, out string? paramValue))
            {
                return paramValue;
            }
            else
            {
                LogManager.LogError($"No se ha podido encontrar el parametro de configuracion con ID: '{paramID}'");
                return null;
            }
        }

        private static void ReadTestFrameworkMainConfigFile()
        {
            try
            {
                using StreamReader reader = new(MAIN_CONFIG_FILE_PATH);
                string line;
                while ((line = reader.ReadLine()!) != null)
                {
                    // Deletes everything after "#" in the line
                    int commentIndex = line.IndexOf("#");
                    if (commentIndex >= 0)
                    {
                        line = line[..commentIndex];
                    }

                    line = line.Trim();

                    // Si the line is empty after removing the comment, skip it
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    // Divide the line in key & value using the ':' separator
                    string[] parts = line.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        ConfigParams[key] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer el archivo: {ex.Message}");
            }
        }
    }
}