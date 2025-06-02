using System.Text.Json.Serialization;

namespace QArantine.Code.JsonContexts
{
    [JsonSerializable(typeof(object))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    public partial class TFProfilerJsonContext : JsonSerializerContext {}
}
