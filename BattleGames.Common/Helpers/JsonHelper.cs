using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Stip.BattleGames.Common.Helpers;

public class JsonHelper : IInjectable
{
    public virtual async Task<ActionResult> SerializeToUtf8StreamAsync<T>(
        T model,
        Stream stream,
        JsonTypeInfo<T> typeInfo)
    {
        try
        {
            await JsonSerializer.SerializeAsync(
                stream,
                model,
                typeInfo);
            await stream.FlushAsync();
            return ActionResult.Success;
        }
        catch
        {
            return ActionResult.Failure;
        }
    }

    public virtual async Task<ActionResult<T>> DeserializeFromUtf8StreamAsync<T>(
        Stream stream,
        JsonTypeInfo<T> typeInfo)
    {
        try
        {
            return new ActionResult<T>(await JsonSerializer.DeserializeAsync(stream, typeInfo));
        }
        catch
        {
            return ActionResult<T>.Failure;
        }
    }
}
