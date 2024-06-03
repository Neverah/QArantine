using System;
using System.IO;
using System.Collections.Generic;

namespace TestFramework.Code.FrameworkModules
{
    public static class ConfigManager
    {
        public const string MAIN_CONFIG_FILE_PATH = "TestFramework/Config/TestFrameworkDefault.config";
        public const string OVERWRITE_CONFIG_FILE_PATH = "TestFramework/Config/TestFrameworkOverwrite.config";
        private static readonly Dictionary<string, string> ConfigParams;
        private static bool IsMainConfigAlreadyLoaded = false;

        static ConfigManager()
        {
            ConfigParams = new();
            LoadTestFrameworkMainConfig();
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

        private static void LoadTestFrameworkMainConfig()
        {
            if (IsMainConfigAlreadyLoaded) return;

            ReadTestFrameworkMainConfigFile(MAIN_CONFIG_FILE_PATH);

            if (File.Exists(OVERWRITE_CONFIG_FILE_PATH))
                ReadTestFrameworkMainConfigFile(OVERWRITE_CONFIG_FILE_PATH);
            
            IsMainConfigAlreadyLoaded = true;
            LogOK("TestFramework Main Config loaded");
        }

        private static void ReadTestFrameworkMainConfigFile(string configFilePath)
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
                LogError($"Error reading the file '{configFilePath}': {ex.Message}");
            }
        }
    }
}