using System.Text.Json;
using System.Text.Json.Serialization;

namespace QArantine.Code.FrameworkModules.InputSimulation
{

    public class InputEventJsonConverter : JsonConverter<IInputEvent>
    {
        public override IInputEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            var deviceType = root.GetProperty("DeviceType").GetString();

            return deviceType switch
            {
                "Keyboard" => JsonSerializer.Deserialize<KeyboardEvent>(root.GetRawText(), options) ?? throw new JsonException("Deserialization of KeyboardEvent returned null."),
                "Mouse" => JsonSerializer.Deserialize<MouseEvent>(root.GetRawText(), options) ?? throw new JsonException("Deserialization of MouseEvent returned null."),
                _ => throw new NotSupportedException($"Unknown DeviceType: {deviceType}")
            };
        }

        public override void Write(Utf8JsonWriter writer, IInputEvent value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case KeyboardEvent k:
                    JsonSerializer.Serialize(writer, k, options);
                    break;
                case MouseEvent m:
                    JsonSerializer.Serialize(writer, m, options);
                    break;
                default:
                    throw new NotSupportedException($"Unknown event type: {value.GetType()}");
            }
        }
    }
}