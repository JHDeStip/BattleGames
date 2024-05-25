using AutoFixture;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.BattleGames.Common.Messages;
using Stip.BattleGames.UnitTestsCommon;
using Stip.BeerBattle.Helpers;
using Stip.BeerBattle.Messages;
using Stip.BeerBattle.Windows;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.BeerBattle.UnitTests.Windows;

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
    }

    [TestMethod]
    public async Task ActivateAsync_CorrectlyActivates()
    {
        var fixture = FixtureFactory.Create();

        var mockMessenger = fixture.FreezeMock<IMessenger>();

        var applicationContext = fixture.Freeze<ApplicationContext>();

        var pointsFormatHelper = fixture.Freeze<PointsFormatHelper>();

        var target = fixture.Create<ChartWindowViewModel>();

        await target.ActivateAsync(CancellationToken.None);

        Assert.IsTrue(target.ChartItems.Select(x => x.Name).SequenceEqual(applicationContext.Groups.Select(x => x.Name)));

        mockMessenger.VerifyRegister<PointsUpdatedMessage>(target, Times.Once());
        mockMessenger.VerifyRegister<ToggleChartWindowStateMessage>(target, Times.Once());

        mockMessenger.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Receive_PointsUpdatedMessage()
    {
        var fixture = FixtureFactory.Create();

        var applicationContext = fixture.Freeze<ApplicationContext>();

        var priceFormatHelper = fixture.Freeze<PointsFormatHelper>();

        var target = fixture.Create<ChartWindowViewModel>();

        target.Receive(new PointsUpdatedMessage());

        Assert.IsTrue(target.ChartItems.Select(x => x.Name).SequenceEqual(applicationContext.Groups.Select(x => x.Name)));
    }
}
