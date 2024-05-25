using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.BattleGames.UnitTestsCommon;
using Stip.BeerBattle.Helpers;
using Stip.BeerBattle.Items;
using System.Collections.Generic;

namespace Stip.BeerBattle.UnitTests.Items;

[TestClass]
public class ChartItemTests
{
    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void TotalPoints_NotifiesTotalPointsStringUpdate(
        bool hasValueChanged)
    {
        var target = new ChartItem(null)
        {
            TotalPoints = 123
        };

        var notifiedProperties = new List<string>(2);

        target.PropertyChanged += (s, e) => notifiedProperties.Add(e.PropertyName);

        target.TotalPoints = hasValueChanged
            ? target.TotalPoints + 1
            : target.TotalPoints;

        Assert.AreEqual(hasValueChanged, notifiedProperties.Contains(nameof(target.TotalPointsString)));
    }

    [TestMethod]
    public void TotalPointsString_ReturnsCorrectString()
    {
        var fixture = FixtureFactory.Create();

        var totalPointsString = fixture.Create<string>();

        var mockPointsFormatHelper = fixture.FreezeMock<PointsFormatHelper>();
        mockPointsFormatHelper
            .Setup(x => x.Format(It.IsAny<decimal>()))
            .Returns(totalPointsString);

        var target = fixture
            .Build<ChartItem>()
            .With(x => x.TotalPoints, 123)
            .Create();

        var actual = target.TotalPointsString;

        Assert.AreEqual(totalPointsString, actual);

        mockPointsFormatHelper.Verify(x => x.Format(123), Times.Once);

        mockPointsFormatHelper.VerifyNoOtherCalls();
    }
}
