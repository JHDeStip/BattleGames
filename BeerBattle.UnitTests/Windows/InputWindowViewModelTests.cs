using AutoFixture;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.BattleGames.Common;
using Stip.BattleGames.Common.Factories;
using Stip.BattleGames.Common.Messages;
using Stip.BattleGames.Common.Services;
using Stip.BattleGames.UnitTestsCommon;
using Stip.BeerBattle.Factories;
using Stip.BeerBattle.Helpers;
using Stip.BeerBattle.Items;
using Stip.BeerBattle.Messages;
using Stip.BeerBattle.Models;
using Stip.BeerBattle.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.BeerBattle.UnitTests.Windows;

[TestClass]
public class InputWindowViewModelTests
{
    [TestMethod]
    public async Task InitializeAsync_CorrectlyInitializes()
    {
        var fixture = FixtureFactory.Create();

        var applicationContext = fixture.Freeze<ApplicationContext>();

        var target = fixture.Create<InputWindowViewModel>();

        await target.InitializeAsync(CancellationToken.None);

        Assert.AreEqual(applicationContext.Config.WindowBackgroundColor, target.BackgroundColor);
    }

    [TestMethod]
    public async Task ActivateAsync_CorrectlyActivates()
    {
        var fixture = FixtureFactory.Create();

        var mockMessenger = fixture.FreezeMock<IMessenger>();

        var applicationContext = fixture.Freeze<ApplicationContext>();
        var inputItems = fixture.CreateMany<InputItem>(3).ToList();
        var totalPointsStrings = fixture.CreateMany<string>(2).ToList();

        decimal totalPoints = 0;
        inputItems.ForEach(x => totalPoints += x.Product.PointsPerItem * x.Amount);

        Action totalPointsChangedCallback = null;
        var mockInputItemsFactory = fixture.FreezeMock<InputItemsFactory>();
        mockInputItemsFactory
            .Setup(x => x.Create(
                It.IsAny<IEnumerable<Product>>(),
                It.IsAny<Action>()))
            .Callback<IEnumerable<Product>, Action>(
                (_, x) => totalPointsChangedCallback = x)
            .Returns(inputItems);

        var mockPointsFormatHelper = fixture.FreezeMock<PointsFormatHelper>();
        mockPointsFormatHelper
            .SetupSequence(x => x.Format(It.IsAny<decimal>()))
            .Returns(totalPointsStrings[0])
            .Returns(totalPointsStrings[1]);

        var target = fixture.Create<InputWindowViewModel>();

        void VerifyNoOtherCalls()
        {
            mockMessenger.VerifyNoOtherCalls();
            mockInputItemsFactory.VerifyNoOtherCalls();
            mockPointsFormatHelper.VerifyNoOtherCalls();
        }

        await target.ActivateAsync(CancellationToken.None);

        Assert.IsTrue(target.GroupItems.Select(x => (x.Group, x.Name, x.Color)).SequenceEqual(applicationContext.Groups.Select(x => (x, x.Name, x.Color))));
        Assert.IsTrue(target.InputItems.SequenceEqual(inputItems));
        Assert.AreEqual(totalPointsStrings[0], target.TotalPointsString);

        mockInputItemsFactory.Verify(
            x => x.Create(
                applicationContext.Products,
                It.IsAny<Action>()),
            Times.Once);

        mockPointsFormatHelper.Verify(x => x.Format(totalPoints), Times.Once);

        VerifyNoOtherCalls();

        totalPointsChangedCallback();

        mockPointsFormatHelper.Verify(x => x.Format(totalPoints), Times.Exactly(2));
        Assert.AreEqual(totalPointsStrings[1], target.TotalPointsString);

        VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task DeactivateAsync_CorrectlyDeactivates()
    {
        var fixture = FixtureFactory.Create();

        var mockMessenger = fixture.FreezeMock<IMessenger>();

        var mockChartWindowViewModel = fixture.FreezeMock<ChartWindowViewModel>();

        var mockAsyncServiceScope = fixture.FreezeMock<ServiceScopeFactory.AsyncServiceScopeWrapper>();
        mockAsyncServiceScope
            .Setup(x => x.GetRequiredService<ChartWindowViewModel>())
            .Returns(mockChartWindowViewModel.Object);

        var mockServiceScopeFactory = fixture.FreezeMock<ServiceScopeFactory>();
        mockServiceScopeFactory
            .Setup(x => x.CreateAsyncScope())
            .Returns(mockAsyncServiceScope.Object);

        var target = fixture.Create<InputWindowViewModel>();

        await target.DeactivateAsync(CancellationToken.None);

        mockMessenger.Verify(x => x.UnregisterAll(target), Times.Once);

        mockServiceScopeFactory.Verify(x => x.CreateAsyncScope(), Times.Once);
        mockAsyncServiceScope.Verify(x => x.GetRequiredService<ChartWindowViewModel>(), Times.Once);
        mockChartWindowViewModel.Verify(x => x.CloseAsync(CancellationToken.None), Times.Once);
        mockAsyncServiceScope.Verify(x => x.DisposeAsync(), Times.Once);

        mockMessenger.VerifyNoOtherCalls();
        mockServiceScopeFactory.VerifyNoOtherCalls();
        mockAsyncServiceScope.VerifyNoOtherCalls();
        mockChartWindowViewModel.VerifyNoOtherCalls();
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task CanCloseAsync_ReturnsIfCanClose(
        bool canClose)
    {
        var fixture = FixtureFactory.Create();

        var mockDialogService = fixture.FreezeMock<DialogService>();
        mockDialogService
            .Setup(x => x.ShowYesNoDialogAsync(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(canClose);

        var target = fixture.Create<InputWindowViewModel>();

        var actual = await target.CanDeactivateAsync(CancellationToken.None);

        mockDialogService.Verify(
            x => x.ShowYesNoDialogAsync(
                BattleGames.Common.UIStrings.Input_AreYouSure,
                BattleGames.Common.UIStrings.Input_AreYouSureYouWantToClose),
            Times.Once);

        Assert.AreEqual(canClose, actual);

        mockDialogService.VerifyNoOtherCalls();
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task CommitOrder_CorrectlyCommitsOrder(bool saveAsyncSuccess)
    {
        var fixture = FixtureFactory.Create();

        var applicationContext = fixture.Freeze<ApplicationContext>();

        var mockDisableUIService = fixture.FreezeMockDisableUIService();

        var mockInputItemsFactory = fixture.FreezeMock<InputItemsFactory>();

        var mockPointsCalculator = fixture.FreezeMock<PointsCalculator>();

        var mockMessenger = fixture.FreezeMock<IMessenger>();

        var mockPointsFormatHelper = fixture.FreezeMock<PointsFormatHelper>();

        var mockDataPersistenceHelper = fixture.FreezeMock<DataPersistenceHelper>();
        mockDataPersistenceHelper
            .Setup(x => x.SaveDataAsync())
            .ReturnsAsync(ActionResult.FromSuccessState(saveAsyncSuccess));

        var mockDialogService = fixture.FreezeMock<DialogService>();

        var target = fixture.Create<InputWindowViewModel>();

        void VerifyNoOtherCalls()
        {
            mockDisableUIService.VerifyNoOtherCalls();
            mockInputItemsFactory.VerifyNoOtherCalls();
            mockPointsCalculator.VerifyNoOtherCalls();
            mockMessenger.VerifyNoOtherCalls();
            mockPointsFormatHelper.VerifyNoOtherCalls();
            mockDataPersistenceHelper.VerifyNoOtherCalls();
            mockDialogService.VerifyNoOtherCalls();
        }

        var referenceAmounts = target.InputItems.Select(x => x.Amount).ToList();
        var referencePointsPerItems = target.InputItems.Select(x => x.Product.PointsPerItem).ToList();
        var referenceTotalPoints = target.GroupItems.Select(x => x.Group.TotalPoints).ToList();

        await target.CommitOrder(target.GroupItems[1]);

        foreach (var inputItem in target.InputItems)
        {
            Assert.AreEqual(0, inputItem.Amount);
        }

        Assert.AreEqual(referenceTotalPoints[0], target.GroupItems[0].Group.TotalPoints);
        Assert.AreEqual(
            referenceTotalPoints[1] + Enumerable.Range(0, 3).Sum(x => referenceAmounts[x] * referencePointsPerItems[x]),
            target.GroupItems[1].Group.TotalPoints);
        Assert.AreEqual(referenceTotalPoints[2], target.GroupItems[2].Group.TotalPoints);

        mockDisableUIService.VerifyUIDisabledAndEnabledAtLeastOnce();

        mockPointsCalculator.Verify(x => x.CalculatePointLevels(applicationContext.Groups), Times.Once);

        mockMessenger.VerifySend<PointsUpdatedMessage>(Times.Once());

        mockDataPersistenceHelper.Verify(x => x.SaveDataAsync(), Times.Once);

        if (!saveAsyncSuccess)
        {
            mockDialogService.Verify(x => x.ShowErrorAsync(BattleGames.Common.UIStrings.Error_CannotSaveData), Times.Once);
        }

        VerifyNoOtherCalls();
    }

    [DataTestMethod]
    [DataRow(true, true)]
    [DataRow(true, false)]
    [DataRow(false, true)]
    public async Task Reset_CorrectlyResets(
        bool shouldReset,
        bool saveAsyncSuccess)
    {
        var fixture = FixtureFactory.Create();

        var applicationContext = fixture.Freeze<ApplicationContext>();

        var mockDialogService = fixture.FreezeMock<DialogService>();
        mockDialogService
            .Setup(x => x.ShowYesNoDialogAsync(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(shouldReset);

        var mockDisableUIService = fixture.FreezeMockDisableUIService();

        var mockPointsCalculator = fixture.FreezeMock<PointsCalculator>();

        var mockMessenger = fixture.FreezeMock<IMessenger>();

        var mockPointsFormatHelper = fixture.FreezeMock<PointsFormatHelper>();

        var mockDataPersistenceHelper = fixture.FreezeMock<DataPersistenceHelper>();
        mockDataPersistenceHelper
            .Setup(x => x.SaveDataAsync())
            .ReturnsAsync(ActionResult.FromSuccessState(saveAsyncSuccess));

        var target = fixture.Create<InputWindowViewModel>();

        void VerifyNoOtherCalls()
        {
            mockDialogService.VerifyNoOtherCalls();
            mockDisableUIService.VerifyNoOtherCalls();
            mockMessenger.VerifyNoOtherCalls();
            mockPointsFormatHelper.VerifyNoOtherCalls();
            mockDataPersistenceHelper.VerifyNoOtherCalls();
        }

        await target.Reset();

        mockDialogService.Verify(
            x => x.ShowYesNoDialogAsync(
                BattleGames.Common.UIStrings.Input_AreYouSure,
                BattleGames.Common.UIStrings.Input_AreYouSureYouWantToReset),
            Times.Once);

        if (shouldReset)
        {
            mockDisableUIService.VerifyUIDisabledAndEnabledAtLeastOnce();

            mockPointsCalculator.Verify(x => x.Reset(applicationContext.Groups), Times.Once);

            mockMessenger.VerifySend<PointsUpdatedMessage>(Times.Once());

            mockDataPersistenceHelper.Verify(x => x.SaveDataAsync(), Times.Once);

            if (!saveAsyncSuccess)
            {
                mockDialogService.Verify(x => x.ShowErrorAsync(BattleGames.Common.UIStrings.Error_CannotSaveData), Times.Once);
            }
        }

        VerifyNoOtherCalls();
    }

    [TestMethod]
    public void ToggleChartWindowState_CorrectlyTogglesWindowState()
    {
        var fixture = FixtureFactory.Create();

        var mockMessenger = fixture.FreezeMock<IMessenger>();

        var target = fixture.Create<InputWindowViewModel>();

        target.ToggleChartWindowState();

        mockMessenger.VerifySend<ToggleChartWindowStateMessage>(Times.Once());

        mockMessenger.VerifyNoOtherCalls();
    }
}
