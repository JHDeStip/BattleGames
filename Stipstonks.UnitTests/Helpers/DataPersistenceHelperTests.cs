using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.BattleGames.UnitTestsCommon;
using Stip.BattleGames.Common;
using Stip.BattleGames.Common.Helpers;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.JsonModels;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stip.Stipstonks.UnitTests.Helpers
{
    [TestClass]
    public class DataPersistenceHelperTests
    {
        [DataTestMethod]
        [DataRow(true, true)]
        [DataRow(true, false)]
        [DataRow(false, true)]
        public async Task LoadDataAsync_CorrectlyLoadsData(
            bool openStreamSuccess,
            bool deserializeFromUtf8StreamAsyncSuccess)
        {
            var fixture = FixtureFactory.Create();

            var executableDirectory = fixture.Create<string>();
            using var fileStream = new MemoryStream();
            var jsonData = fixture.Create<Data>();

            var applicationContext = fixture.Freeze<ApplicationContext>();

            var mockEnvironmentHelper = fixture.FreezeMock<EnvironmentHelper>();
            mockEnvironmentHelper
                .SetupGet(x => x.ExecutableDirectory)
                .Returns(executableDirectory);

            var mockFileHelper = fixture.FreezeMock<FileHelper>();
            mockFileHelper
                .Setup(x => x.OpenStream(It.IsAny<string>(), It.IsAny<FileMode>()))
                .Returns(new ActionResult<Stream>(openStreamSuccess, fileStream));

            var mockJsonHelper = fixture.FreezeMock<JsonHelper>();
            mockJsonHelper
                .Setup(x => x.DeserializeFromUtf8StreamAsync<Data>(It.IsAny<Stream>()))
                .ReturnsAsync(new ActionResult<Data>(deserializeFromUtf8StreamAsyncSuccess, jsonData));

            var target = fixture.Create<DataPersistenceHelper>();

            void VerifyNoOtherCalls()
            {
                mockEnvironmentHelper.VerifyNoOtherCalls();
                mockFileHelper.VerifyNoOtherCalls();
                mockJsonHelper.VerifyNoOtherCalls();
            }

            var actual = await target.LoadDataAsync();

            Assert.AreEqual(openStreamSuccess && deserializeFromUtf8StreamAsyncSuccess, actual.IsSuccess);

            if (openStreamSuccess
                && deserializeFromUtf8StreamAsyncSuccess)
            {
                Assert.AreEqual(jsonData.ToConfig(), applicationContext.Config);
                Assert.IsTrue(jsonData.Products.Select(x => x.ToModel()).SequenceEqual(applicationContext.Products));
            }

            mockEnvironmentHelper.VerifyGet(x => x.ExecutableDirectory, Times.Once);

            mockFileHelper.Verify(x => x.OpenStream(Path.Combine(executableDirectory, "Data.json"), FileMode.Open), Times.Once);

            if (!openStreamSuccess)
            {
                VerifyNoOtherCalls();
                return;
            }

            mockJsonHelper.Verify(x => x.DeserializeFromUtf8StreamAsync<Data>(fileStream), Times.Once);

            if (!deserializeFromUtf8StreamAsyncSuccess)
            {
                VerifyNoOtherCalls();
                return;
            }
        }

        [DataTestMethod]
        [DataRow(true, true)]
        [DataRow(true, false)]
        [DataRow(false, true)]
        public async Task SaveDataAsync_CorrectlySavesData(
            bool openStreamSuccess,
            bool serializeToUtf8StreamAsyncSuccess)
        {
            var fixture = FixtureFactory.Create();

            var executableDirectory = fixture.Create<string>();
            using var fileStream = new MemoryStream();

            var applicationContext = fixture.Freeze<ApplicationContext>();

            var mockEnvironmentHelper = fixture.FreezeMock<EnvironmentHelper>();
            mockEnvironmentHelper
                .SetupGet(x => x.ExecutableDirectory)
                .Returns(executableDirectory);

            var mockFileHelper = fixture.FreezeMock<FileHelper>();
            mockFileHelper
                .Setup(x => x.OpenStream(It.IsAny<string>(), It.IsAny<FileMode>()))
                .Returns(new ActionResult<Stream>(openStreamSuccess, fileStream));

            var mockJsonHelper = fixture.FreezeMock<JsonHelper>();
            mockJsonHelper
                .Setup(x => x.SerializeToUtf8StreamAsync(It.IsAny<object>(), It.IsAny<Stream>()))
                .ReturnsAsync(ActionResult.FromSuccessState(serializeToUtf8StreamAsyncSuccess));

            var target = fixture.Create<DataPersistenceHelper>();

            void VerifyNoOtherCalls()
            {
                mockEnvironmentHelper.VerifyNoOtherCalls();
                mockFileHelper.VerifyNoOtherCalls();
                mockJsonHelper.VerifyNoOtherCalls();
            }

            var actual = await target.SaveDataAsync();

            Assert.AreEqual(openStreamSuccess && serializeToUtf8StreamAsyncSuccess, actual.IsSuccess);

            mockEnvironmentHelper.VerifyGet(x => x.ExecutableDirectory, Times.Once);

            mockFileHelper.Verify(x => x.OpenStream(Path.Combine(executableDirectory, "Data.json"), FileMode.Create), Times.Once);

            if (!openStreamSuccess)
            {
                VerifyNoOtherCalls();
                return;
            }

            mockJsonHelper.Verify(
                x => x.SerializeToUtf8StreamAsync(
                    It.Is<Data>(x
                        => x.PriceResolutionInCents == applicationContext.Config.PriceResolutionInCents
                        && x.Products.Count == applicationContext.Products.Count),
                    fileStream),
                Times.Once);
        }
    }
}
