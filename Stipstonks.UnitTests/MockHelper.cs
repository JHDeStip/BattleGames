using Stip.Stipstonks.Services;
using Moq;

namespace Stip.Stipstonks.UnitTests
{
    public static class MockHelper
    {
        public static Mock<DisableUIService> GetMockDisableUIService()
        {
            var mock = new Mock<DisableUIService>();

            mock.Setup(x => x.Disable())
                .Returns(mock.Object);

            return mock;
        }
    }
}
