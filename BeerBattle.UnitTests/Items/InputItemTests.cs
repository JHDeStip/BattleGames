using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stip.BattleGames.UnitTestsCommon;
using Stip.BeerBattle.Items;
using System.Collections.Generic;

namespace Stip.BeerBattle.UnitTests.Items;

[TestClass]
public class InputItemTests
{
    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Amount_CallsCallback(
        bool hasValueChanged)
    {
        var fixture = FixtureFactory.Create();

        var wasCallbackCalled = false;

        var target = fixture
            .Build<InputItem>()
            .With(x => x.Amount, 123)
            .With(x => x.TotalPointsChangedCallback, () => wasCallbackCalled = true)
            .Create();

        target.Amount = hasValueChanged
            ? target.Amount + 1
            : target.Amount;

        Assert.AreEqual(hasValueChanged, wasCallbackCalled);
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Decrement_CorrectlyDecrements(
        bool isAmount0)
    {
        var fixture = FixtureFactory.Create();

        var wasCallbackCalled = false;

        var target = fixture
            .Build<InputItem>()
            .With(x => x.Amount, isAmount0 ? 0 : 123)
            .With(x => x.TotalPointsChangedCallback, () => wasCallbackCalled = true)
            .Create();

        var notifiedProperties = new List<string>(3);

        target.PropertyChanged += (s, e) => notifiedProperties.Add(e.PropertyName);

        target.Decrement();

        Assert.AreEqual(isAmount0 ? 0 : 122, target.Amount);

        Assert.AreEqual(!isAmount0, notifiedProperties.Contains(nameof(target.Amount)));
        Assert.AreEqual(!isAmount0, wasCallbackCalled);
    }

    [DataTestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(123)]
    public void Increment_CorrectlyIncrements(
        int initialAmount)
    {
        var fixture = FixtureFactory.Create();

        var wasCallbackCalled = false;

        var target = fixture
            .Build<InputItem>()
            .With(x => x.Amount, initialAmount)
            .With(x => x.TotalPointsChangedCallback, () => wasCallbackCalled = true)
            .Create();

        var notifiedProperties = new List<string>(3);

        target.PropertyChanged += (s, e) => notifiedProperties.Add(e.PropertyName);

        target.Increment();

        Assert.AreEqual(initialAmount + 1, target.Amount);

        Assert.IsTrue(notifiedProperties.Contains(nameof(target.Amount)));
        Assert.IsTrue(wasCallbackCalled);
    }
}
