using Stip.Stipstonks.Common;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Helpers
{
    public class JsonHelper : IInjectable
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions;

        static JsonHelper()
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        }

        public virtual async Task<ActionResult> SerializeToUtf8StreamAsync(object model, Stream stream)
        {
            try
            {
                await JsonSerializer.SerializeAsync(stream, model, _jsonSerializerOptions);
                await stream.FlushAsync();
                return ActionResult.Success;
            }
            catch
            {
                return ActionResult.Failure;
            }
        }

        public virtual async Task<ActionResult<T>> DeserializeFromUtf8StreamAsync<T>(Stream stream)
        {
            try
            {
                return new ActionResult<T>(await JsonSerializer.DeserializeAsync<T>(stream, _jsonSerializerOptions));
            }
            catch
            {
                return ActionResult<T>.Failure;
            }
        }
    }
}
