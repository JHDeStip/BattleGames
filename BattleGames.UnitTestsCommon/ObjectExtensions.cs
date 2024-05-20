using System.Linq;
using System.Text.Json;

namespace Stip.BattleGames.UnitTestsCommon;

public static class ObjectExtensions
{
    public static bool DeeplyEquals<T>(this T instance, T other)
        => JsonSerializer.SerializeToUtf8Bytes(instance)
            .SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(other));
}
