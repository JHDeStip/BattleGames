using AutoFixture;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.BattleGames.Common.Messages;
using Stip.BattleGames.UnitTestsCommon;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Items;
using Stip.Stipstonks.Messages;
using Stip.Stipstonks.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.UnitTests.Windows;

[TestClass]
public class ChartWindowViewModelTests
{
    [TestMethod]
    public async Task OnInitializeAsync_CorrectlyInitializes()
    {
        var fixture = FixtureFactory.Create();
        
        var applicationContext = fixture.Freeze<ApplicationContext>();

        var target = fixture.Create<ChartWindowViewModel>();

        await target.InitializeAsync(CancellationToken.None);

        Assert.AreEqual(applicationContext.Config.WindowBackgroundColor, target.BackgroundColor);

        Assert.AreEqual(applicationContext.Config.PriceUpdateProgressBarColor, target.PriceUpdateProgressItem.Color);

        Assert.AreEqual(applicationContext.Config.CrashProgressBarColor, target.CrashProgressItem.Color);
        Assert.AreEqual(applicationContext.Config.CrashInterval, target.CrashProgressItem.Duration);
    }

    [TestMethod]
    public async Task ActivateAsync_CorrectlyActivates()
    {
        var fixture = FixtureFactory.Create();

        var mockMessenger = fixture.FreezeMock<IMessenger>();

        var applicationContext = fixture.Freeze<ApplicationContext>();

        var priceFormatHelper = fixture.Freeze<PriceFormatHelper>();

        var target = fixture.Create<ChartWindowViewModel>();

        await target.ActivateAsync(CancellationToken.None);

        Assert.IsTrue(target.ChartItems.Select(x => x.Name).SequenceEqual(applicationContext.Products.Select(x => x.Name)));

        mockMessenger.VerifyRegister<PricesUpdatedMessage>(target, Times.Once());
        mockMessenger.VerifyRegister<ToggleChartWindowStateMessage>(target, Times.Once());
        mockMessenger.VerifyRegister<StartedMessage>(target, Times.Once());
        mockMessenger.VerifyRegister<StoppedMessage>(target, Times.Once());

        mockMessenger.VerifyNoOtherCalls();
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Receive_PricesUpdatedMessage(
        bool hasCrashed)
    {
        var fixture = FixtureFactory.Create();

        var applicationContext = fixture.Freeze<ApplicationContext>();
        applicationContext.HasCrashed = hasCrashed;

        var priceFormatHelper = fixture.Freeze<PriceFormatHelper>();

        var target = fixture.Create<ChartWindowViewModel>();
        target.PriceUpdateProgressItem.IsRunning = true;
        target.CrashProgressItem.IsRunning = true;

        var setPriceUpdateProgressItemRunningStates = new List<bool>();
        target.PriceUpdateProgressItem.PropertyChanged +=
            (s, e) =>
            {
                if (e.PropertyName == nameof(StonkMarketEventProgressItem.IsRunning))
                {
                    setPriceUpdateProgressItemRunningStates.Add(target.PriceUpdateProgressItem.IsRunning);
                }
            };

        var setCrashProgressItemRunningStates = new List<bool>();
        target.CrashProgressItem.PropertyChanged +=
            (s, e) =>
            {
                if (e.PropertyName == nameof(StonkMarketEventProgressItem.IsRunning))
                {
                    setCrashProgressItemRunningStates.Add(target.CrashProgressItem.IsRunning);
                }
            };

        target.Receive(new PricesUpdatedMessage());

        Assert.IsTrue(target.ChartItems.Select(x => x.Name).SequenceEqual(applicationContext.Products.Select(x => x.Name)));

        Assert.AreEqual(2, setPriceUpdateProgressItemRunningStates.Count);
        Assert.IsFalse(setPriceUpdateProgressItemRunningStates[0]);
        Assert.IsTrue(setPriceUpdateProgressItemRunningStates[1]);

        var expectedDuration = hasCrashed
            ? applicationContext.Config.CrashDuration
            : applicationContext.Config.PriceUpdateInterval;

        Assert.AreEqual(expectedDuration, target.PriceUpdateProgressItem.Duration);

        if (hasCrashed)
        {
            Assert.AreEqual(2, setCrashProgressItemRunningStates.Count);
            Assert.IsFalse(setCrashProgressItemRunningStates[0]);
            Assert.IsTrue(setCrashProgressItemRunningStates[1]);

            Assert.AreEqual(applicationContext.Config.CrashChartWindowBackgroundColor, target.BackgroundColor);
        }
        else
        {
            Assert.AreEqual(0, setCrashProgressItemRunningStates.Count);

            Assert.AreEqual(applicationContext.Config.WindowBackgroundColor, target.BackgroundColor);
        }
    }

    [TestMethod]
    public void Receive_StartedMessage_CorrectlyHandlesMessage()
    {
        var fixture = FixtureFactory.Create();

        var applicationContext = fixture.Freeze<ApplicationContext>();

        var target = fixture.Create<ChartWindowViewModel>();
        target.PriceUpdateProgressItem.IsRunning = false;
        target.CrashProgressItem.IsRunning = false;

        target.Receive(new StartedMessage());

        Assert.AreEqual(target.BackgroundColor, applicationContext.Config.WindowBackgroundColor);
        Assert.AreEqual(target.PriceUpdateProgressItem.Duration, applicationContext.Config.PriceUpdateInterval);
        Assert.IsTrue(target.PriceUpdateProgressItem.IsRunning);
        Assert.IsTrue(target.CrashProgressItem.IsRunning);
    }

    [TestMethod]
    public void Receive_StoppedMessage_CorrectlyHandlesMessage()
    {
        var fixture = FixtureFactory.Create();

        var target = fixture.Create<ChartWindowViewModel>();
        target.PriceUpdateProgressItem.IsRunning = true;
        target.CrashProgressItem.IsRunning = true;

        target.Receive(new StoppedMessage());

        Assert.IsFalse(target.PriceUpdateProgressItem.IsRunning);
        Assert.IsFalse(target.CrashProgressItem.IsRunning);
    }
}
