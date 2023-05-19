using AutoFixture;
using Caliburn.Micro;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Items;
using Stip.Stipstonks.Messages;
using Stip.Stipstonks.Windows;
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
        private class TestChartWindowViewModel : ChartWindowViewModel
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
            var target = new ChartWindowViewModel();

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

            var mockEventAggregator = fixture.FreezeMock<IEventAggregator>();

            var applicationContext = fixture.Freeze<ApplicationContext>();

            var priceFormatHelper = fixture.Freeze<PriceFormatHelper>();

            var target = fixture.Create<TestChartWindowViewModel>();

            await target.OnActivateAsync(CancellationToken.None);

            Assert.IsTrue(target.ChartItems.Select(x => x.Name).SequenceEqual(applicationContext.Products.Select(x => x.Name)));

            foreach (var chartItem in target.ChartItems)
            {
                Assert.AreSame(priceFormatHelper, chartItem.PriceFormatHelper);
            }

            mockEventAggregator.VerifySubscribeOnce(target);

            mockEventAggregator.VerifyNoOtherCalls();
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task OnDeactivateAsync_CorrectlyDeactivates(
            bool close)
        {
            var fixture = FixtureFactory.Create();

            var mockEventAggregator = fixture.FreezeMock<IEventAggregator>();

            var target = fixture.Create<TestChartWindowViewModel>();

            await target.OnDeactivateAsync(
                close,
                default);

            mockEventAggregator.Verify(x => x.Unsubscribe(target), Times.Once);

            mockEventAggregator.VerifyNoOtherCalls();
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
        public async Task HandleAsync_PricesUpdatedMessage(
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

            await target.HandleAsync(
                new PricesUpdatedMessage(),
                CancellationToken.None);

            Assert.IsTrue(target.ChartItems.Select(x => x.Name).SequenceEqual(applicationContext.Products.Select(x => x.Name)));

            foreach (var chartItem in target.ChartItems)
            {
                Assert.AreSame(priceFormatHelper, chartItem.PriceFormatHelper);
            }

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
                Assert.IsFalse(setCrashProgressItemRunningStates.Any());

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
        public async Task HandleAsync_ToggleChartWindowStateMessage_CorrectlyHandlesMessage(
            WindowStyle initialWindowStyle,
            WindowState initialWindowState)
        {
            var fixture = FixtureFactory.Create();

            var target = fixture
                .Build<ChartWindowViewModel>()
                .With(x => x.WindowStyle, initialWindowStyle)
                .With(x => x.WindowState, initialWindowState)
                .Create();

            await target.HandleAsync(
                new ToggleChartWindowStateMessage(),
                CancellationToken.None);

            Assert.AreEqual(WindowStyle.None, target.WindowStyle);
            Assert.AreEqual(WindowState.Maximized, target.WindowState);

            await target.HandleAsync(
                new ToggleChartWindowStateMessage(),
                CancellationToken.None);

            Assert.AreEqual(initialWindowStyle, target.WindowStyle);
            Assert.AreEqual(initialWindowState, target.WindowState);
        }

        [TestMethod]
        public async Task HandleAsync_StatedMessage_CorrectlyHandlesMessage()
        {
            var fixture = FixtureFactory.Create();

            var applicationContext = fixture.Freeze<ApplicationContext>();

            var target = fixture.Create<ChartWindowViewModel>();
            target.PriceUpdateProgressItem.IsRunning = false;
            target.CrashProgressItem.IsRunning = false;

            await target.HandleAsync(
                new StartedMessage(),
                CancellationToken.None);

            Assert.AreEqual(target.BackgroundColor, applicationContext.Config.WindowBackgroundColor);
            Assert.AreEqual(target.PriceUpdateProgressItem.Duration.TimeSpan, applicationContext.Config.PriceUpdateInterval);
            Assert.IsTrue(target.PriceUpdateProgressItem.IsRunning);
            Assert.IsTrue(target.CrashProgressItem.IsRunning);
        }

        [TestMethod]
        public async Task HandleAsync_StoppedMessage_CorrectlyHandlesMessage()
        {
            var fixture = FixtureFactory.Create();

            var target = fixture.Create<ChartWindowViewModel>();
            target.PriceUpdateProgressItem.IsRunning = true;
            target.CrashProgressItem.IsRunning = true;

            await target.HandleAsync(
                new StoppedMessage(),
                CancellationToken.None);

            Assert.IsFalse(target.PriceUpdateProgressItem.IsRunning);
            Assert.IsFalse(target.CrashProgressItem.IsRunning);
        }
    }
}
