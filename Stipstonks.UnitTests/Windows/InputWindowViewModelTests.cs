﻿using AutoFixture;
using Caliburn.Micro;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.Stipstonks.Common;
using Stip.Stipstonks.Factories;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Items;
using Stip.Stipstonks.Messages;
using Stip.Stipstonks.Models;
using Stip.Stipstonks.Services;
using Stip.Stipstonks.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.UnitTests.Windows
{
    [TestClass]
    public class InputWindowViewModelTests
    {
        internal class TestInputWindowViewModel(
            ApplicationContext _applicationContext,
            IMessenger _messenger,
            DataPersistenceHelper _dataPersistenceHelper,
            StonkMarketManager _stonkMarketManager,
            PriceCalculator _priceCalculator,
            PriceFormatHelper _priceFormatHelper,
            DisableUIService _disableUIService,
            DialogService _dialogService,
            ServiceScopeFactory _serviceScopeFactory,
            InputItemsFactory _inputItemsFactory)
            : InputWindowViewModel(
                _applicationContext,
                _messenger,
                _dataPersistenceHelper,
                _stonkMarketManager,
                _priceCalculator,
                _priceFormatHelper,
                _disableUIService,
                _dialogService,
                _serviceScopeFactory,
                _inputItemsFactory)
        {
            public new Task OnInitializeAsync(CancellationToken ct)
                => base.OnInitializeAsync(ct);

            public new Task OnActivateAsync(CancellationToken ct)
                => base.OnActivateAsync(ct);

            public new Task OnDeactivateAsync(bool close, CancellationToken ct)
                => base.OnDeactivateAsync(close, ct);
        }

        [TestMethod]
        public void Constructor_CorrectlyConstructs()
        {
            var fixture = FixtureFactory.Create();

            var target = fixture
                .Build<InputWindowViewModel>()
                .OmitAutoProperties()
                .Create();

            Assert.AreEqual(UIStrings.Global_ApplicationName, target.DisplayName);
        }

        [TestMethod]
        public async Task OnInitializeAsync_CorrectlyInitializes()
        {
            var fixture = FixtureFactory.Create();

            var applicationContext = fixture.Freeze<ApplicationContext>();

            var target = fixture.Create<TestInputWindowViewModel>();

            await target.OnInitializeAsync(CancellationToken.None);

            Assert.AreEqual(applicationContext.Config.WindowBackgroundColor, target.BackgroundColor);
        }

        [TestMethod]
        public async Task OnActivateAsync_CorrectlyActivates()
        {
            var fixture = FixtureFactory.Create();

            var mockMessenger = fixture.FreezeMock<IMessenger>();

            var applicationContext = fixture.Freeze<ApplicationContext>();
            var inputItems = fixture.CreateMany<InputItem>(3).ToList();
            var totalPriceStrings = fixture.CreateMany<string>(2).ToList();

            var totalPrice = 0;
            inputItems.ForEach(x => totalPrice += x.PriceInCents * x.Amount);

            System.Action totalPriceChangedCallback = null;
            var mockInputItemsFactory = fixture.FreezeMock<InputItemsFactory>();
            mockInputItemsFactory
                .Setup(x => x.Create(
                    It.IsAny<IEnumerable<Product>>(),
                    It.IsAny<IEnumerable<InputItem>>(),
                    It.IsAny<System.Action>()))
                .Callback<IEnumerable<Product>, IEnumerable<InputItem>, System.Action>(
                    (_, _, x) => totalPriceChangedCallback = x)
                .Returns(inputItems);

            var mockPriceFormatHelper = fixture.FreezeMock<PriceFormatHelper>();
            mockPriceFormatHelper
                .SetupSequence(x => x.Format(It.IsAny<int>()))
                .Returns(totalPriceStrings[0])
                .Returns(totalPriceStrings[1]);

            var target = fixture.Create<TestInputWindowViewModel>();

            void VerifyNoOtherCalls()
            {
                mockMessenger.VerifyNoOtherCalls();
                mockInputItemsFactory.VerifyNoOtherCalls();
                mockPriceFormatHelper.VerifyNoOtherCalls();
            }

            var initialInputItems = target.InputItems;

            await target.OnActivateAsync(CancellationToken.None);

            Assert.IsTrue(target.InputItems.SequenceEqual(inputItems));
            Assert.AreEqual(totalPriceStrings[0], target.TotalPriceString);

            mockMessenger.VerifyRegister(target, Times.Once());

            mockInputItemsFactory.Verify(
                x => x.Create(
                    applicationContext.Products,
                    initialInputItems,
                    It.IsAny<System.Action>()),
                Times.Once);

            mockPriceFormatHelper.Verify(x => x.Format(totalPrice), Times.Once);

            VerifyNoOtherCalls();

            totalPriceChangedCallback();

            mockPriceFormatHelper.Verify(x => x.Format(totalPrice), Times.Exactly(2));
            Assert.AreEqual(totalPriceStrings[1], target.TotalPriceString);

            VerifyNoOtherCalls();
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task OnDeactivateAsync_CorrectlyDeactivates(bool close)
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

            var target = fixture.Create<TestInputWindowViewModel>();

            await target.OnDeactivateAsync(
                close,
                CancellationToken.None);

            mockMessenger.Verify(x => x.UnregisterAll(target), Times.Once);

            if (close)
            {
                mockServiceScopeFactory.Verify(x => x.CreateAsyncScope(), Times.Once);
                mockAsyncServiceScope.Verify(x => x.GetRequiredService<ChartWindowViewModel>(), Times.Once);
                mockChartWindowViewModel.Verify(x => x.TryCloseAsync(null), Times.Once);
                mockAsyncServiceScope.Verify(x => x.DisposeAsync(), Times.Once);
            }

            mockChartWindowViewModel.VerifySet(x => x.IsNotifying = It.IsAny<bool>(), Times.Once);
            mockChartWindowViewModel.VerifySet(x => x.DisplayName = It.IsAny<string>(), Times.Once);

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
                .Setup(x => x.ShowYesNoDialog(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(canClose);

            var target = fixture.Create<InputWindowViewModel>();

            var actual = await target.CanCloseAsync(CancellationToken.None);

            mockDialogService.Verify(
                x => x.ShowYesNoDialog(
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

            System.Action totalPriceChangedCallback = null;
            var mockInputItemsFactory = fixture.FreezeMock<InputItemsFactory>();
            mockInputItemsFactory
                .Setup(x => x.Create(
                    It.IsAny<IEnumerable<Product>>(),
                    It.IsAny<IEnumerable<InputItem>>(),
                    It.IsAny<System.Action>()))
                .Callback<IEnumerable<Product>, IEnumerable<InputItem>, System.Action>(
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

            var target = fixture.Create<TestInputWindowViewModel>();

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
                    It.IsAny<System.Action>()),
                Times.Once);

            mockPriceFormatHelper.Verify(x => x.Format(totalPrice), Times.Once);

            mockDataPersistenceHelper.Verify(x => x.SaveDataAsync(), Times.Once);

            if (!saveAsyncSuccess)
            {
                mockDialogService.Verify(x => x.ShowError(UIStrings.Error_CannotSaveData), Times.Once);
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
                .Setup(x => x.ShowYesNoDialog(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(shouldStop);

            var mockDisableUIService = fixture.FreezeMockDisableUIService();

            var mockStonkMarketManager = fixture.FreezeMock<StonkMarketManager>();

            var mockMessenger = fixture.FreezeMock<IMessenger>();

            var target = fixture
                .Build<InputWindowViewModel>()
                .With(x => x.IsRunning, true)
                .Create();

            await target.Stop();

            mockDialogService.Verify(
                x => x.ShowYesNoDialog(
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
                .Setup(x => x.ShowYesNoDialog(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(shouldReset);

            var mockDisableUIService = fixture.FreezeMockDisableUIService();

            var mockPriceCalculator = fixture.FreezeMock<PriceCalculator>();

            var mockMessenger = fixture.FreezeMock<IMessenger>();

            System.Action totalPriceChangedCallback = null;
            var mockInputItemsFactory = fixture.FreezeMock<InputItemsFactory>();
            mockInputItemsFactory
                .Setup(x => x.Create(
                    It.IsAny<IEnumerable<Product>>(),
                    It.IsAny<IEnumerable<InputItem>>(),
                    It.IsAny<System.Action>()))
                .Callback<IEnumerable<Product>, IEnumerable<InputItem>, System.Action>(
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
                x => x.ShowYesNoDialog(
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
                    It.IsAny<System.Action>()),
                Times.Once);

                mockPriceFormatHelper.Verify(x => x.Format(totalPrice), Times.Once);

                mockDataPersistenceHelper.Verify(x => x.SaveDataAsync(), Times.Once);

                if (!saveAsyncSuccess)
                {
                    mockDialogService.Verify(x => x.ShowError(UIStrings.Error_CannotSaveData), Times.Once);
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

            System.Action totalPriceChangedCallback = null;
            var mockInputItemsFactory = fixture.FreezeMock<InputItemsFactory>();
            mockInputItemsFactory
                .Setup(x => x.Create(
                    It.IsAny<IEnumerable<Product>>(),
                    It.IsAny<IEnumerable<InputItem>>(),
                    It.IsAny<System.Action>()))
                .Callback<IEnumerable<Product>, IEnumerable<InputItem>, System.Action>(
                    (_, _, x) => totalPriceChangedCallback = x)
                .Returns(inputItems);

            var mockPriceFormatHelper = fixture.FreezeMock<PriceFormatHelper>();
            mockPriceFormatHelper
                .SetupSequence(x => x.Format(It.IsAny<int>()))
                .Returns(totalPriceStrings[0])
                .Returns(totalPriceStrings[1]);

            var target = fixture.Create<TestInputWindowViewModel>();
            target.InputItems.Apply(x => x.Amount = 0);
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
                    It.IsAny<System.Action>()),
                Times.Once);

            mockPriceFormatHelper.Verify(x => x.Format(totalPrice), Times.Once);

            VerifyNoOtherCalls();

            totalPriceChangedCallback();

            mockPriceFormatHelper.Verify(x => x.Format(totalPrice), Times.Exactly(2));
            Assert.AreEqual(totalPriceStrings[1], target.TotalPriceString);

            VerifyNoOtherCalls();
        }
    }
}
