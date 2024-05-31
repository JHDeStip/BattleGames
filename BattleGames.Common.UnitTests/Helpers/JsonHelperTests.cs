using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stip.BattleGames.Common.Helpers;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Stip.BattleGames.Common.UnitTests.Helpers;

[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(ExampleModel))]
[JsonSerializable(typeof(string))]
internal partial class ExampleJsonContext : JsonSerializerContext { }

internal class ExampleModel
{
    public string Value1 { get; set; }
    public int Value2 { get; set; }
}

[TestClass]
public class JsonHelperTests
{
    private const string ExampleJsonString = "{\r\n  \"value1\": \"ExampleValue1\",\r\n  \"value2\": 123\r\n}";

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task SerializeToUtf8StreamAsync_CorrectlySerializes(
        bool canWriteStream)
    {
        var model = new ExampleModel
        {
            Value1 = "ExampleValue1",
            Value2 = 123
        };

        using var stream = new MemoryStream();
        if (!canWriteStream)
        {
            stream.Dispose();
        }

        var target = new JsonHelper();

        var actual = await target.SerializeToUtf8StreamAsync(
            model,
            stream,
            ExampleJsonContext.Default.ExampleModel);

        Assert.AreEqual(canWriteStream, actual.IsSuccess);

        if (canWriteStream)
        {
            var serialized = Encoding.ASCII.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            Assert.AreEqual(ExampleJsonString, serialized);
        }
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task DeserializeFromUtf8StreamAsync_CorrectlyDeserializes(
        bool canReadStream)
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(ExampleJsonString));
        if (!canReadStream)
        {
            stream.Dispose();
        }

        var target = new JsonHelper();

        var actual = await target.DeserializeFromUtf8StreamAsync(
            stream,
            ExampleJsonContext.Default.ExampleModel);

        Assert.AreEqual(canReadStream, actual.IsSuccess);

        if (canReadStream)
        {
            Assert.AreEqual("ExampleValue1", actual.Data.Value1);
            Assert.AreEqual(123, actual.Data.Value2);
        }
    }

    [TestMethod]
    public async Task DeserializeFromUtf8StreamAsync_DoesNotDeserializeWrongType()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(ExampleJsonString));

        var target = new JsonHelper();

        var actual = await target.DeserializeFromUtf8StreamAsync(
            stream,
            ExampleJsonContext.Default.String);

        Assert.IsFalse(actual.IsSuccess);
    }

    [TestMethod]
    public async Task DeserializeFromUtf8StreamAsync_DoesNotDeserializeMalformedJson()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(ExampleJsonString[1..]));

        var target = new JsonHelper();

        var actual = await target.DeserializeFromUtf8StreamAsync(
            stream,
            ExampleJsonContext.Default.ExampleModel);

        Assert.IsFalse(actual.IsSuccess);
    }
}
