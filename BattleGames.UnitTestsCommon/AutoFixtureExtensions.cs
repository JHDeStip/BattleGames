using AutoFixture;
using Moq;
using Stip.BattleGames.Common.Services;
using System.Linq;

namespace Stip.BattleGames.UnitTestsCommon;

public static class AutoFixtureExtensions
{
    public static Mock<T> FreezeMock<T>(this Fixture fixture, MockBehavior behavior = MockBehavior.Default)
        where T : class
    {
        var parameters = Enumerable
            .Range(0, typeof(T).GetConstructors().FirstOrDefault()?.GetParameters().Length ?? 0)
            .Select(_ => (object)null)
            .ToArray();

        var mock = new Mock<T>(behavior, parameters);
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
