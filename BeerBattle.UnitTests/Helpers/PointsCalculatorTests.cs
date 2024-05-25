using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stip.BattleGames.UnitTestsCommon;
using Stip.BeerBattle.Helpers;
using Stip.BeerBattle.Models;
using System.Linq;

namespace Stip.BeerBattle.UnitTests.Helpers;

[TestClass]
public class PointsCalculatorTests
{
    [TestMethod]
    public void Reset_CorrectlyResets()
    {
        var fixture = FixtureFactory.Create();

        var groups = fixture.CreateMany<Group>(3).ToList();

        var target = fixture.Create<PointsCalculator>();

        target.Reset(groups);

        foreach (var group in groups)
        {
            Assert.AreEqual(0, group.TotalPoints);
            Assert.AreEqual(1, group.Level);
        }
    }

    [TestMethod]
    public void CalculatePointLevels_CorrectlyCalculatesPointLevels()
    {
        var fixture = FixtureFactory.Create();

        var groups = fixture.CreateMany<Group>(3).ToList();
        groups[0].TotalPoints = 3;
        groups[1].TotalPoints = 8;
        groups[2].TotalPoints = 5;

        var referenceGroups = groups.Select(x => x with { }).ToList();

        var target = fixture.Create<PointsCalculator>();

        target.CalculatePointLevels(groups);

        Assert.AreEqual(3 / (double)8, groups[0].Level);
        Assert.AreEqual(1, groups[1].Level);
        Assert.AreEqual(5 / (double)8, groups[2].Level);

        for (var i = 0; i < referenceGroups.Count; ++i)
        {
            groups[i].Level = referenceGroups[i].Level;
        }

        Assert.IsTrue(groups.DeeplyEquals(referenceGroups));
    }

    [TestMethod]
    public void CalculatePointLevels_CorrectlyCalculatesPointLevelsWith0Cents()
    {
        var fixture = FixtureFactory.Create();

        var groups = fixture
            .Build<Group>()
            .With(x => x.TotalPoints, 0)
            .CreateMany()
            .ToList();

        var referenceGroups = groups.Select(x => x with { }).ToList();

        var target = fixture.Create<PointsCalculator>();

        target.CalculatePointLevels(groups);

        for (var i = 0; i < referenceGroups.Count; ++i)
        {
            Assert.AreEqual(1, groups[i].Level);
            groups[i].Level = referenceGroups[i].Level;
        }

        Assert.IsTrue(groups.DeeplyEquals(referenceGroups));
    }
}
