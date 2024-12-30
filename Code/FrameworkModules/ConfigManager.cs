using System.Text.RegularExpressions;

namespace QArantine.Code.FrameworkModules
{
    public static class ConfigManager
    {
        public const string MAIN_CONFIG_FILE_PATH = "QArantine/Config/QArantineDefault.config";
        public const string OVERWRITE_CONFIG_FILE_PATH = "QArantine/Config/QArantineOverwrite.config";
        private static readonly Dictionary<string, string> ConfigParams;
        private static bool IsMainConfigAlreadyLoaded = false;

        static ConfigManager()
        {
            ConfigParams = [];
            LoadQArantineMainConfig();
        }

        public static string? GetTFConfigParamAsString(string paramID)
        {
            if (ConfigParams.TryGetValue(paramID, out string? paramValue))
            {
                return paramValue;
            }
            else
            {
                LogError($"The configuration parameter with ID '{paramID}' could not be found");
                return null;
            }
        }

        public static bool GetTFConfigParamAsBool(string paramID)
        {
            if (ConfigParams.TryGetValue(paramID, out string? paramValue))
            {
                if (bool.TryParse(paramValue, out bool boolValue)) return boolValue;
                LogError($"Could not parse the configuration parameter '{paramID}' to bool, returning the default value: 'false'");
                return false;
            }
            else
            {
                LogError($"The configuration parameter with ID '{paramID}' could not be found");
                return false;
            }
        }

        public static int? GetTFConfigParamAsInt(string paramID)
        {
            if (ConfigParams.TryGetValue(paramID, out string? paramValue))
            {
                if (int.TryParse(paramValue, out int intValue)) return intValue;
                LogError($"Could not parse the configuration parameter '{paramID}' to int, returning the default value: 'null'");
                return null;
            }
            else
            {
                LogError($"The configuration parameter with ID '{paramID}' could not be found");
                return null;
            }
        }

        private static void LoadQArantineMainConfig()
        {
            if (IsMainConfigAlreadyLoaded) return;

            bool fileLoaded = false;

            fileLoaded = ReadQArantineMainConfigFile(MAIN_CONFIG_FILE_PATH);

            if (File.Exists(OVERWRITE_CONFIG_FILE_PATH))
                fileLoaded = ReadQArantineMainConfigFile(OVERWRITE_CONFIG_FILE_PATH);
            
            IsMainConfigAlreadyLoaded = true;
            if (fileLoaded)
                LogOK("QArantine Main Config loaded");
            else
                LogError("QArantine Main Config could not be loaded");
        }

        private static bool ReadQArantineMainConfigFile(string configFilePath)
        {
            try
            {
                using StreamReader reader = new(configFilePath);
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error reading the file '{configFilePath}': {ex.Message}");
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
            return true;
        }

        public static void UpdateConfigFileValue(string key, string newValue)
        {
            // Expresión regular para identificar la clave y su valor
            string pattern = $@"^(\s*{Regex.Escape(key)}\s*:\s*)([^#\n\r]*)(.*)$";

            // Crear un archivo temporal
            string tempFilePath = Path.GetTempFileName();

            using (var input = new FileStream(MAIN_CONFIG_FILE_PATH, FileMode.Open, FileAccess.Read))
            using (var output = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            using (var reader = new StreamReader(input))
            using (var writer = new StreamWriter(output))
            {
                string? line;
                bool updated = false;

                while ((line = reader.ReadLine()) != null)
                {
                    var match = Regex.Match(line, pattern);
                    if (match.Success && !updated)
                    {
                        // Escribir la nueva línea con el nuevo valor, preservando el comentario
                        writer.WriteLine($"{match.Groups[1].Value}{newValue}{match.Groups[3].Value}");
                        updated = true; // Asegurarse de actualizar solo la primera ocurrencia
                    }
                    else
                    {
                        // Escribir la línea original
                        writer.WriteLine(line);
                    }
                }
            }

            // Reemplazar el archivo original con el archivo temporal
            File.Delete(MAIN_CONFIG_FILE_PATH);
            File.Move(tempFilePath, MAIN_CONFIG_FILE_PATH);
        }
    }
}