using AutoFixture;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.Stipstonks.Common;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.JsonModels;
using System.IO;
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
            var fileStream = new MemoryStream();
            var jsonData = fixture.Create<Data>();
            var data = fixture.Create<Models.Data>();

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

            var mockMapper = fixture.FreezeMock<IMapper>();
            mockMapper
                .Setup(x => x.Map<Models.Data>(It.IsAny<object>()))
                .Returns(data);

            var target = fixture.Create<DataPersistenceHelper>();

            void VerifyNoOtherCalls()
            {
                mockEnvironmentHelper.VerifyNoOtherCalls();
                mockFileHelper.VerifyNoOtherCalls();
                mockJsonHelper.VerifyNoOtherCalls();
                mockMapper.VerifyNoOtherCalls();
            }

            var actual = await target.LoadDataAsync();

            Assert.AreEqual(openStreamSuccess && deserializeFromUtf8StreamAsyncSuccess, actual.IsSuccess);

            if (openStreamSuccess
                && deserializeFromUtf8StreamAsyncSuccess)
            {
                Assert.AreSame(data.Config, applicationContext.Config);
                Assert.AreSame(data.Products, applicationContext.Products);
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

            mockMapper.Verify(x => x.Map<Models.Data>(jsonData), Times.Once);
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
            var fileStream = new MemoryStream();
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

            var mockMapper = fixture.FreezeMock<IMapper>();
            mockMapper
                .Setup(x => x.Map<Data>(It.IsAny<object>()))
                .Returns(jsonData);

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
                mockMapper.VerifyNoOtherCalls();
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

            mockMapper.Verify(x => x.Map<Data>(It.Is<Models.Data>(
                y => y.Config == applicationContext.Config
                && y.Products == applicationContext.Products)), Times.Once);

            mockJsonHelper.Verify(x => x.SerializeToUtf8StreamAsync(jsonData, fileStream), Times.Once);
        }
    }
}
