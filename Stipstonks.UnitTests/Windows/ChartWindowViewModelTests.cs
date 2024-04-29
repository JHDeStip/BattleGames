using AutoFixture;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Items;
using Stip.Stipstonks.Messages;
using Stip.Stipstonks.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Stip.Stipstonks.UnitTests.Windows
{
    [TestClass]
    public class ChartWindowViewModelTests
    {
        internal class TestChartWindowViewModel(
            ApplicationContext _applicationContext,
            IMessenger _messenger,
            PriceFormatHelper _priceFormatHelper)
            : ChartWindowViewModel(
                _applicationContext,
                _messenger,
                _priceFormatHelper)
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
                .Build<ChartWindowViewModel>()
                .OmitAutoProperties()
                .Create();

            Assert.AreEqual(UIStrings.Global_ApplicationName, target.DisplayName);
        }

        [TestMethod]
        public async Task OnInitializeAsync_CorrectlyInitializes()
        {
            var fixture = FixtureFactory.Create();
            
            var applicationContext = fixture.Freeze<ApplicationContext>();

            var target = fixture.Create<TestChartWindowViewModel>();

            await target.OnInitializeAsync(CancellationToken.None);

            Assert.AreEqual(applicationContext.Config.WindowBackgroundColor, target.BackgroundColor);

            Assert.AreEqual(applicationContext.Config.PriceUpdateProgressBarColor, target.PriceUpdateProgressItem.Color);

            Assert.AreEqual(applicationContext.Config.CrashProgressBarColor, target.CrashProgressItem.Color);
            Assert.AreEqual(applicationContext.Config.CrashInterval, target.CrashProgressItem.Duration.TimeSpan);
        }

        [TestMethod]
        public async Task OnActivateAsync_CorrectlyActivates()
        {
            var fixture = FixtureFactory.Create();

            var mockMessenger = fixture.FreezeMock<IMessenger>();

            var applicationContext = fixture.Freeze<ApplicationContext>();

            var priceFormatHelper = fixture.Freeze<PriceFormatHelper>();

            var target = fixture.Create<TestChartWindowViewModel>();

            await target.OnActivateAsync(CancellationToken.None);

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
        public async Task OnDeactivateAsync_CorrectlyDeactivates(
            bool close)
        {
            var fixture = FixtureFactory.Create();

            var mockMessenger = fixture.FreezeMock<IMessenger>();

            var target = fixture.Create<TestChartWindowViewModel>();

            await target.OnDeactivateAsync(
                close,
                default);

            mockMessenger.Verify(x => x.UnregisterAll(target), Times.Once);

            mockMessenger.VerifyNoOtherCalls();
        }

        [DataTestMethod]
        [DataRow(true, null)]
        [DataRow(true, true)]
        [DataRow(true, false)]
        [DataRow(false, null)]
        public async Task CanCloseAsync_ReturnsCorrectly(
            bool tryCloseAsyncCalled,
            bool? dialogResult)
        {
            var fixture = FixtureFactory.Create();

            var target = fixture.Create<ChartWindowViewModel>();

            if (tryCloseAsyncCalled)
            {
                await target.TryCloseAsync(dialogResult);
            }

            var actual = await target.CanCloseAsync(CancellationToken.None);

            Assert.AreEqual(tryCloseAsyncCalled, actual);
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
            target.PriceUpdateProgressItem.IsNotifying = true;
            target.PriceUpdateProgressItem.IsRunning = true;
            target.CrashProgressItem.IsNotifying = true;
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

            Assert.AreEqual(expectedDuration, target.PriceUpdateProgressItem.Duration.TimeSpan);

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

        [DataTestMethod]
        [DataRow(WindowStyle.SingleBorderWindow, WindowState.Normal)]
        [DataRow(WindowStyle.SingleBorderWindow, WindowState.Minimized)]
        [DataRow(WindowStyle.SingleBorderWindow, WindowState.Maximized)]
        [DataRow(WindowStyle.ToolWindow, WindowState.Normal)]
        [DataRow(WindowStyle.ToolWindow, WindowState.Minimized)]
        [DataRow(WindowStyle.ToolWindow, WindowState.Maximized)]
        [DataRow(WindowStyle.ThreeDBorderWindow, WindowState.Normal)]
        [DataRow(WindowStyle.ThreeDBorderWindow, WindowState.Minimized)]
        [DataRow(WindowStyle.ThreeDBorderWindow, WindowState.Maximized)]
        public void Receive_ToggleChartWindowStateMessage_CorrectlyHandlesMessage(
            WindowStyle initialWindowStyle,
            WindowState initialWindowState)
        {
            var fixture = FixtureFactory.Create();

            var target = fixture
                .Build<ChartWindowViewModel>()
                .With(x => x.WindowStyle, initialWindowStyle)
                .With(x => x.WindowState, initialWindowState)
                .Create();

            target.Receive(new ToggleChartWindowStateMessage());

            Assert.AreEqual(WindowStyle.None, target.WindowStyle);
            Assert.AreEqual(WindowState.Maximized, target.WindowState);

            target.Receive(new ToggleChartWindowStateMessage());

            Assert.AreEqual(initialWindowStyle, target.WindowStyle);
            Assert.AreEqual(initialWindowState, target.WindowState);
        }

        [TestMethod]
        public void Receive_StatedMessage_CorrectlyHandlesMessage()
        {
            var fixture = FixtureFactory.Create();

            var applicationContext = fixture.Freeze<ApplicationContext>();

            var target = fixture.Create<ChartWindowViewModel>();
            target.PriceUpdateProgressItem.IsRunning = false;
            target.CrashProgressItem.IsRunning = false;

            target.Receive(new StartedMessage());

            Assert.AreEqual(target.BackgroundColor, applicationContext.Config.WindowBackgroundColor);
            Assert.AreEqual(target.PriceUpdateProgressItem.Duration.TimeSpan, applicationContext.Config.PriceUpdateInterval);
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
}
