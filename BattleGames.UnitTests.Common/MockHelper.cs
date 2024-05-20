using Moq;
using Stip.BattleGames.Common.Services;

namespace Stip.BattleGames.UnitTestsCommon
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
