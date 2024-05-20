using AutoFixture;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.BattleGames.Common;
using Stip.BattleGames.Common.Factories;
using Stip.BattleGames.Common.Services;
using Stip.BattleGames.UnitTestsCommon;
using Stip.Stipstonks.Factories;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Items;
using Stip.Stipstonks.Messages;
using Stip.Stipstonks.Models;
using Stip.Stipstonks.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.UnitTests.Windows;

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
        var totalPriceStrings = fixture.CreateMany<string>(2).ToList();

        var totalPrice = 0;
        inputItems.ForEach(x => totalPrice += x.PriceInCents * x.Amount);

        Action totalPriceChangedCallback = null;
        var mockInputItemsFactory = fixture.FreezeMock<InputItemsFactory>();
        mockInputItemsFactory
            .Setup(x => x.Create(
                It.IsAny<IEnumerable<Product>>(),
                It.IsAny<IEnumerable<InputItem>>(),
                It.IsAny<Action>()))
            .Callback<IEnumerable<Product>, IEnumerable<InputItem>, Action>(
                (_, _, x) => totalPriceChangedCallback = x)
            .Returns(inputItems);

        var mockPriceFormatHelper = fixture.FreezeMock<PriceFormatHelper>();
        mockPriceFormatHelper
            .SetupSequence(x => x.Format(It.IsAny<int>()))
            .Returns(totalPriceStrings[0])
            .Returns(totalPriceStrings[1]);

        var target = fixture.Create<InputWindowViewModel>();

        void VerifyNoOtherCalls()
        {
            mockMessenger.VerifyNoOtherCalls();
            mockInputItemsFactory.VerifyNoOtherCalls();
            mockPriceFormatHelper.VerifyNoOtherCalls();
        }

        var initialInputItems = target.InputItems;

        await target.ActivateAsync(CancellationToken.None);

        Assert.IsTrue(target.InputItems.SequenceEqual(inputItems));
        Assert.AreEqual(totalPriceStrings[0], target.TotalPriceString);

        mockMessenger.VerifyRegister(target, Times.Once());

        mockInputItemsFactory.Verify(
            x => x.Create(
                applicationContext.Products,
                initialInputItems,
                It.IsAny<Action>()),
            Times.Once);

        mockPriceFormatHelper.Verify(x => x.Format(totalPrice), Times.Once);

        VerifyNoOtherCalls();

        totalPriceChangedCallback();

        mockPriceFormatHelper.Verify(x => x.Format(totalPrice), Times.Exactly(2));
        Assert.AreEqual(totalPriceStrings[1], target.TotalPriceString);

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
                UIStrings.Input_AreYouSure,
                UIStrings.Input_AreYouSureYouWantToClose),
            Times.Once);

        Assert.AreEqual(canClose, actual);

        mockDialogService.VerifyNoOtherCalls();
    }

    [DataTestMethod]
    [DataRow(true, true)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(false, false)]
    public async Task CommitOrder_CorrectlyCommitsOrder(
        bool hasCrashed,
        bool saveAsyncSuccess)
    {
        var fixture = FixtureFactory.Create();

        var applicationContext = fixture.Freeze<ApplicationContext>();
        applicationContext.HasCrashed = hasCrashed;

        var inputItems = fixture.CreateMany<InputItem>(3).ToList();
        var totalPriceStrings = fixture.CreateMany<string>(2).ToList();

        var totalPrice = 0;
        inputItems.ForEach(x => totalPrice += x.PriceInCents * x.Amount);

        var mockDisableUIService = fixture.FreezeMockDisableUIService();

        Action totalPriceChangedCallback = null;
        var mockInputItemsFactory = fixture.FreezeMock<InputItemsFactory>();
        mockInputItemsFactory
            .Setup(x => x.Create(
                It.IsAny<IEnumerable<Product>>(),
                It.IsAny<IEnumerable<InputItem>>(),
                It.IsAny<Action>()))
            .Callback<IEnumerable<Product>, IEnumerable<InputItem>, Action>(
                (_, _, x) => totalPriceChangedCallback = x)
            .Returns(inputItems);

        var mockPriceFormatHelper = fixture.FreezeMock<PriceFormatHelper>();
        mockPriceFormatHelper
            .SetupSequence(x => x.Format(It.IsAny<int>()))
            .Returns(totalPriceStrings[0])
            .Returns(totalPriceStrings[1]);

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
            mockPriceFormatHelper.VerifyNoOtherCalls();
            mockDataPersistenceHelper.VerifyNoOtherCalls();
            mockDialogService.VerifyNoOtherCalls();
        }

        var initialInputItems = target.InputItems;
        var referenceAmounts = initialInputItems.Select(x => x.Amount).ToList();
        var referenceTotalAmountsSold = initialInputItems.Select(x => x.Product.TotalAmountSold).ToList();
        var referenceVirtualAmountsSold = initialInputItems.Select(x => x.Product.VirtualAmountSold).ToList();

        await target.CommitOrder();

        for (var i = 0; i < initialInputItems.Count; ++i)
        {
            Assert.AreEqual(
                referenceTotalAmountsSold[i] + referenceAmounts[i],
                initialInputItems[i].Product.TotalAmountSold);

            var expectedVirtualAmountSold = hasCrashed
                ? referenceVirtualAmountsSold[i]
                : referenceVirtualAmountsSold[i] + referenceAmounts[i];

            Assert.AreEqual(expectedVirtualAmountSold, initialInputItems[i].Product.VirtualAmountSold);

            Assert.AreEqual(0, initialInputItems[i].Amount);
        }

        Assert.IsTrue(target.InputItems.SequenceEqual(inputItems));
        Assert.AreEqual(totalPriceStrings[0], target.TotalPriceString);

        mockDisableUIService.VerifyUIDisabledAndEnabledAtLeastOnce();

        mockInputItemsFactory.Verify(
            x => x.Create(
                applicationContext.Products,
                initialInputItems,
                It.IsAny<Action>()),
            Times.Once);

        mockPriceFormatHelper.Verify(x => x.Format(totalPrice), Times.Once);

        mockDataPersistenceHelper.Verify(x => x.SaveDataAsync(), Times.Once);

        if (!saveAsyncSuccess)
        {
            mockDialogService.Verify(x => x.ShowErrorAsync(UIStrings.Error_CannotSaveData), Times.Once);
        }

        VerifyNoOtherCalls();

        totalPriceChangedCallback();

        mockPriceFormatHelper.Verify(x => x.Format(totalPrice), Times.Exactly(2));
        Assert.AreEqual(totalPriceStrings[1], target.TotalPriceString);

        VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Start_CorrectlyStarts()
    {
        var fixture = FixtureFactory.Create();

        var mockStonkMarketManager = fixture.FreezeMock<StonkMarketManager>();

        var mockMessenger = fixture.FreezeMock<IMessenger>();

        var target = fixture
            .Build<InputWindowViewModel>()
            .With(x => x.IsRunning, false)
            .Create();

        target.Start();

        Assert.IsTrue(target.IsRunning);

        mockStonkMarketManager.Verify(x => x.Start(), Times.Once);

        mockMessenger.VerifySend<StartedMessage>(Times.Once());

        mockStonkMarketManager.VerifyNoOtherCalls();
        mockMessenger.VerifyNoOtherCalls();
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task Stop_CorrectlyStops(bool shouldStop)
    {
        var fixture = FixtureFactory.Create();

        var mockDialogService = fixture.FreezeMock<DialogService>();
        mockDialogService
            .Setup(x => x.ShowYesNoDialogAsync(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(shouldStop);

        var mockDisableUIService = fixture.FreezeMockDisableUIService();

        var mockStonkMarketManager = fixture.FreezeMock<StonkMarketManager>();

        var mockMessenger = fixture.FreezeMock<IMessenger>();

        var target = fixture
            .Build<InputWindowViewModel>()
            .With(x => x.IsRunning, true)
            .Create();

        await target.Stop();

        mockDialogService.Verify(
            x => x.ShowYesNoDialogAsync(
                UIStrings.Input_AreYouSure,
                UIStrings.Input_AreYouSureYouWantToStop),
            Times.Once);

        Assert.AreEqual(!shouldStop, target.IsRunning);

        if (shouldStop)
        {
            mockDisableUIService.VerifyUIDisabledAndEnabledAtLeastOnce();

            mockStonkMarketManager.Verify(x => x.StopAsync(), Times.Once);

            mockMessenger.VerifySend<StoppedMessage>(Times.Once());
        }

        mockDialogService.VerifyNoOtherCalls();
        mockDisableUIService.VerifyNoOtherCalls();
        mockStonkMarketManager.VerifyNoOtherCalls();
        mockMessenger.VerifyNoOtherCalls();
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

        var inputItems = fixture.CreateMany<InputItem>(3).ToList();
        var totalPriceStrings = fixture.CreateMany<string>(2).ToList();

        var totalPrice = 0;
        inputItems.ForEach(x => totalPrice += x.PriceInCents * x.Amount);

        var mockDialogService = fixture.FreezeMock<DialogService>();
        mockDialogService
            .Setup(x => x.ShowYesNoDialogAsync(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(shouldReset);

        var mockDisableUIService = fixture.FreezeMockDisableUIService();

        var mockPriceCalculator = fixture.FreezeMock<PriceCalculator>();

        var mockMessenger = fixture.FreezeMock<IMessenger>();

        Action totalPriceChangedCallback = null;
        var mockInputItemsFactory = fixture.FreezeMock<InputItemsFactory>();
        mockInputItemsFactory
            .Setup(x => x.Create(
                It.IsAny<IEnumerable<Product>>(),
                It.IsAny<IEnumerable<InputItem>>(),
                It.IsAny<Action>()))
            .Callback<IEnumerable<Product>, IEnumerable<InputItem>, Action>(
                (_, _, x) => totalPriceChangedCallback = x)
            .Returns(inputItems);
        
        var mockPriceFormatHelper = fixture.FreezeMock<PriceFormatHelper>();
        mockPriceFormatHelper
            .SetupSequence(x => x.Format(It.IsAny<int>()))
            .Returns(totalPriceStrings[0])
            .Returns(totalPriceStrings[1]);

        var mockDataPersistenceHelper = fixture.FreezeMock<DataPersistenceHelper>();
        mockDataPersistenceHelper
            .Setup(x => x.SaveDataAsync())
            .ReturnsAsync(ActionResult.FromSuccessState(saveAsyncSuccess));

        var target = fixture.Create<InputWindowViewModel>();

        void VerifyNoOtherCalls()
        {
            mockDialogService.VerifyNoOtherCalls();
            mockDisableUIService.VerifyNoOtherCalls();
            mockPriceCalculator.VerifyNoOtherCalls();
            mockMessenger.VerifyNoOtherCalls();
            mockInputItemsFactory.VerifyNoOtherCalls();
            mockPriceFormatHelper.VerifyNoOtherCalls();
            mockDataPersistenceHelper.VerifyNoOtherCalls();
        }

        var initialInputItems = target.InputItems;

        await target.Reset();

        mockDialogService.Verify(
            x => x.ShowYesNoDialogAsync(
                UIStrings.Input_AreYouSure,
                UIStrings.Input_AreYouSureYouWantToReset),
            Times.Once);

        if (shouldReset)
        {
            Assert.IsTrue(target.InputItems.SequenceEqual(inputItems));
            Assert.AreEqual(totalPriceStrings[0], target.TotalPriceString);

            mockDisableUIService.VerifyUIDisabledAndEnabledAtLeastOnce();

            mockPriceCalculator.Verify(x => x.ResetEntirely(applicationContext.Products), Times.Once);

            mockMessenger.VerifySend<PricesUpdatedMessage>(Times.Once());

            mockInputItemsFactory.Verify(
            x => x.Create(
                applicationContext.Products,
                initialInputItems,
                It.IsAny<Action>()),
            Times.Once);

            mockPriceFormatHelper.Verify(x => x.Format(totalPrice), Times.Once);

            mockDataPersistenceHelper.Verify(x => x.SaveDataAsync(), Times.Once);

            if (!saveAsyncSuccess)
            {
                mockDialogService.Verify(x => x.ShowErrorAsync(UIStrings.Error_CannotSaveData), Times.Once);
            }

            VerifyNoOtherCalls();

            totalPriceChangedCallback();

            mockPriceFormatHelper.Verify(x => x.Format(totalPrice), Times.Exactly(2));
            Assert.AreEqual(totalPriceStrings[1], target.TotalPriceString);

            VerifyNoOtherCalls();
        }
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

    [DataTestMethod]
    [DataRow(true, 3, true)]
    [DataRow(true, 1, true)]
    [DataRow(true, 0, true)]
    [DataRow(false, 3, false)]
    [DataRow(false, 1, false)]
    [DataRow(false, 0, true)]
    public void Receive_PricesUpdatedMessage_CorrectlyHandlesMessage(
        bool allowPriceUpdatesDuringOrder,
        int secondProductAmount,
        bool shouldRefresh)
    {
        var fixture = FixtureFactory.Create();

        var applicationContext = fixture.Freeze<ApplicationContext>();
        applicationContext.Config = applicationContext.Config with { AllowPriceUpdatesDuringOrder = allowPriceUpdatesDuringOrder };

        var inputItems = fixture.CreateMany<InputItem>(3).ToList();
        var totalPriceStrings = fixture.CreateMany<string>(2).ToList();

        var totalPrice = 0;
        inputItems.ForEach(x => totalPrice += x.PriceInCents * x.Amount);

        Action totalPriceChangedCallback = null;
        var mockInputItemsFactory = fixture.FreezeMock<InputItemsFactory>();
        mockInputItemsFactory
            .Setup(x => x.Create(
                It.IsAny<IEnumerable<Product>>(),
                It.IsAny<IEnumerable<InputItem>>(),
                It.IsAny<Action>()))
            .Callback<IEnumerable<Product>, IEnumerable<InputItem>, Action>(
                (_, _, x) => totalPriceChangedCallback = x)
            .Returns(inputItems);

        var mockPriceFormatHelper = fixture.FreezeMock<PriceFormatHelper>();
        mockPriceFormatHelper
            .SetupSequence(x => x.Format(It.IsAny<int>()))
            .Returns(totalPriceStrings[0])
            .Returns(totalPriceStrings[1]);

        var target = fixture.Create<InputWindowViewModel>();

        foreach (var inputItem in target.InputItems)
        {
            inputItem.Amount = 0;
        }
        
        target.InputItems[1].Amount = secondProductAmount;

        void VerifyNoOtherCalls()
        {
            mockInputItemsFactory.VerifyNoOtherCalls();
            mockPriceFormatHelper.VerifyNoOtherCalls();
        }

        var initialInputItems = target.InputItems;

        target.Receive(new PricesUpdatedMessage());

        if (!shouldRefresh)
        {
            Assert.AreSame(initialInputItems, target.InputItems);
            VerifyNoOtherCalls();
            return;
        }

        Assert.IsTrue(target.InputItems.SequenceEqual(inputItems));
        Assert.AreEqual(totalPriceStrings[0], target.TotalPriceString);

        mockInputItemsFactory.Verify(
            x => x.Create(
                applicationContext.Products,
                initialInputItems,
                It.IsAny<Action>()),
            Times.Once);

        mockPriceFormatHelper.Verify(x => x.Format(totalPrice), Times.Once);

        VerifyNoOtherCalls();

        totalPriceChangedCallback();

        mockPriceFormatHelper.Verify(x => x.Format(totalPrice), Times.Exactly(2));
        Assert.AreEqual(totalPriceStrings[1], target.TotalPriceString);

        VerifyNoOtherCalls();
    }
}
