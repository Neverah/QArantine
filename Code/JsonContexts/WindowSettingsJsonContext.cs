using System.Text.Json.Serialization;
using QArantine.Code.QArantineGUI.Models;

namespace QArantine.Code.JsonContexts
{
    [JsonSerializable(typeof(List<WindowSettings>))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    public partial class WindowSettingsJsonContext : JsonSerializerContext {}
}
