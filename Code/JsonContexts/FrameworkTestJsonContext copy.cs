using System.Text.Json;
using System.Text.Json.Serialization;

using QArantine.Code.FrameworkModules;
using QArantine.Code.Test;

namespace QArantine.Code.JsonContexts
{
    [JsonSerializable(typeof(TestResult))]
    [JsonSerializable(typeof(List<TestError>))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(double))]
    [JsonSerializable(typeof(TestError))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    public partial class FrameworkTestJsonContext : JsonSerializerContext 
    {
        public static JsonSerializerOptions GetOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = ConfigManager.GetTFConfigParamAsBool("IndentReportSystemJsonFiles")
            };
        }
    }
}
