using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stip.BattleGames.UnitTestsCommon;
using Stip.BeerBattle.Factories;
using Stip.BeerBattle.Models;
using System.Linq;

namespace Stip.BeerBattle.UnitTests.Factories;

[TestClass]
public class InputItemsFactoryTests
{
    [TestMethod]
    public void Create_CorrectlyCreatesItems()
    {
        var fixture = FixtureFactory.Create();

        var products = fixture.CreateMany<Product>(3).ToList();

        var target = fixture.Create<InputItemsFactory>();

        var totalPointsChangedCallback = () => { };

        var actual = target.Create(
            products,
            totalPointsChangedCallback);

        Assert.IsTrue(actual.Select(x => x.Name).SequenceEqual(products.Select(x => x.Name)));

        foreach (var item in actual)
        {
            Assert.AreSame(totalPointsChangedCallback, item.TotalPointsChangedCallback);
        }
    }
}
