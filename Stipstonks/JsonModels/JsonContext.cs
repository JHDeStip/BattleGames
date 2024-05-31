using System.Text.Json.Serialization;

namespace Stip.Stipstonks.JsonModels
{
    [JsonSourceGenerationOptions(
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        WriteIndented = true,
        UseStringEnumConverter = true)]
    [JsonSerializable(typeof(Data))]
    public partial class JsonContext : JsonSerializerContext { }
}
