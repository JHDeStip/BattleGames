using AutoFixture;
using Moq;
using Stip.Stipstonks.Services;

namespace Stip.Stipstonks.UnitTests
{
    public static class AutoFixtureExtensions
    {
        public static Mock<T> FreezeMock<T>(this Fixture fixture, MockBehavior behavior = MockBehavior.Default)
            where T : class
        {
            var mock = new Mock<T>(behavior);
            fixture.Inject(mock.Object);
            return mock;
        }

        public static Mock<DisableUIService> FreezeMockDisableUIService(this Fixture fixture)
        {
            var mock = MockHelper.GetMockDisableUIService();
            fixture.Inject(mock.Object);
            return mock;
        }
    }
}
