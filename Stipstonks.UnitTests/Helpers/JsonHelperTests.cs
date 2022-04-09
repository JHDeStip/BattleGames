using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stip.Stipstonks.Helpers;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Stip.Stipstonks.UnitTests.Helpers
{
    [TestClass]
    public class JsonHelperTests
    {
        private const string ExampleJsonString = "{\r\n  \"value1\": \"ExampleValue1\",\r\n  \"value2\": 123\r\n}";

        private class ExampleModel
        {
            public string Value1 { get; set; }
            public int Value2 { get; set; }
        }

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

            var stream = new MemoryStream();
            if (!canWriteStream)
            {
                stream.Dispose();
            }

            var target = new JsonHelper();

            var actual = await target.SerializeToUtf8StreamAsync(model, stream);

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
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(ExampleJsonString));
            if (!canReadStream)
            {
                stream.Dispose();
            }

            var target = new JsonHelper();

            var actual = await target.DeserializeFromUtf8StreamAsync<ExampleModel>(stream);

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
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(ExampleJsonString));

            var target = new JsonHelper();

            var actual = await target.DeserializeFromUtf8StreamAsync<string>(stream);

            Assert.IsFalse(actual.IsSuccess);
        }

        [TestMethod]
        public async Task DeserializeFromUtf8StreamAsync_DoesNotDeserializeMalformedJson()
        {
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(ExampleJsonString.Substring(1)));

            var target = new JsonHelper();

            var actual = await target.DeserializeFromUtf8StreamAsync<ExampleModel>(stream);

            Assert.IsFalse(actual.IsSuccess);
        }
    }
}
