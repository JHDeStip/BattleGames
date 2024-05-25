using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.BattleGames.UnitTestsCommon;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Items;
using System.Collections.Generic;

namespace Stip.Stipstonks.UnitTests.Items;

[TestClass]
public class InputItemTests
{
    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Amount_CallsCallback(
        bool hasValueChanged)
    {
        var wasCallbackCalled = false;

        var target = new InputItem(null)
        {
            Amount = 123,
            TotalPriceChangedCallback = () => wasCallbackCalled = true
        };

        var notifiedProperties = new List<string>(2);

        target.PropertyChanged += (s, e) => notifiedProperties.Add(e.PropertyName);

        target.Amount = hasValueChanged
            ? target.Amount + 1
            : target.Amount;

        Assert.AreEqual(hasValueChanged, wasCallbackCalled);
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void PriceInCents_NotifiesPropertyUpdatesAndCallsCallback(
        bool hasValueChanged)
    {
        var wasCallbackCalled = false;

        var target = new InputItem(null)
        {
            PriceInCents = 123,
            TotalPriceChangedCallback = () => wasCallbackCalled = true
        };

        var notifiedProperties = new List<string>(3);

        target.PropertyChanged += (s, e) => notifiedProperties.Add(e.PropertyName);

        target.PriceInCents = hasValueChanged
            ? target.PriceInCents + 1
            : target.PriceInCents;

        Assert.AreEqual(hasValueChanged, notifiedProperties.Contains(nameof(target.PriceString)));
        Assert.AreEqual(hasValueChanged, wasCallbackCalled);
    }

    [TestMethod]
    public void PriceString_ReturnsCorrectString()
    {
        var fixture = FixtureFactory.Create();

        var totalPriceString = fixture.Create<string>();

        var mockPriceFormatHelper = fixture.FreezeMock<PriceFormatHelper>();
        mockPriceFormatHelper
            .Setup(x => x.Format(It.IsAny<int>()))
            .Returns(totalPriceString);

        var target = fixture
            .Build<ChartItem>()
            .With(x => x.PriceInCents, 123)
            .Create();

        var actual = target.PriceString;

        Assert.AreEqual(totalPriceString, actual);

        mockPriceFormatHelper.Verify(x => x.Format(123), Times.Once);

        mockPriceFormatHelper.VerifyNoOtherCalls();
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Decrement_CorrectlyDecrements(
        bool isAmount0)
    {
        var wasCallbackCalled = false;

        var target = new InputItem(null)
        {
            Amount = isAmount0
                ? 0
                : 123,
            TotalPriceChangedCallback = () => wasCallbackCalled = true
        };

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
        var wasCallbackCalled = false;

        var target = new InputItem(null)
        {
            Amount = initialAmount,
            TotalPriceChangedCallback = () => wasCallbackCalled = true
        };

        var notifiedProperties = new List<string>(3);

        target.PropertyChanged += (s, e) => notifiedProperties.Add(e.PropertyName);

        target.Increment();

        Assert.AreEqual(initialAmount + 1, target.Amount);

        Assert.IsTrue(notifiedProperties.Contains(nameof(target.Amount)));
        Assert.IsTrue(wasCallbackCalled);
    }
}
