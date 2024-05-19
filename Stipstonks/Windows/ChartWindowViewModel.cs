using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Items;
using Stip.Stipstonks.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Windows
{
    public class ChartWindowViewModel(
        ApplicationContext _applicationContext,
        IMessenger _messenger,
        PriceFormatHelper _priceFormatHelper)
        : ViewModelBase,
        IRecipient<PricesUpdatedMessage>,
        IRecipient<ToggleChartWindowStateMessage>,
        IRecipient<StartedMessage>,
        IRecipient<StoppedMessage>
    {
        private string _backgroundColor;
        public string BackgroundColor { get => _backgroundColor; set => SetProperty(ref _backgroundColor, value); }

        public StonkMarketEventProgressItem PriceUpdateProgressItem { get; set; } = new();
        public StonkMarketEventProgressItem CrashProgressItem { get; set; } = new();

        private IReadOnlyList<ChartItem> _chartItems = [];
        public IReadOnlyList<ChartItem> ChartItems { get => _chartItems; set => SetProperty(ref _chartItems, value); }

        private WindowState _windowState;
        public WindowState WindowState { get => _windowState; set => SetProperty(ref _windowState, value); }

        private WindowState _previousWindowState;
        private bool canClose;

        public override async ValueTask InitializeAsync(CancellationToken ct)
        {
            await base.InitializeAsync(ct);

            BackgroundColor = _applicationContext.Config.WindowBackgroundColor;

            PriceUpdateProgressItem.Color = _applicationContext.Config.PriceUpdateProgressBarColor;

            CrashProgressItem.Color = _applicationContext.Config.CrashProgressBarColor;
            CrashProgressItem.Duration = _applicationContext.Config.CrashInterval;
        }

        public override async ValueTask ActivateAsync(CancellationToken ct)
        {
            await base.ActivateAsync(ct);

            _messenger.RegisterAll(this);
            RefreshChart();
        }

        public override async ValueTask DeactivateAsync(CancellationToken ct)
        {
            await base.DeactivateAsync(ct);

            _messenger.UnregisterAll(this);
        }

        public override async ValueTask<bool> CanDeactivateAsync(CancellationToken ct)
            => canClose
            && await base.CanDeactivateAsync(ct);

        public override ValueTask CloseAsync(CancellationToken ct)
        {
            canClose = true;
            return base.CloseAsync(ct);
        }

        public void Receive(PricesUpdatedMessage _)
            => Dispatcher.UIThread.Invoke(() =>
            {
                RefreshChart();

                PriceUpdateProgressItem.IsRunning = false;

                PriceUpdateProgressItem.Duration = _applicationContext.HasCrashed
                    ? _applicationContext.Config.CrashDuration
                    : _applicationContext.Config.PriceUpdateInterval;

                PriceUpdateProgressItem.IsRunning = true;
                if (_applicationContext.HasCrashed)
                {
                    CrashProgressItem.IsRunning = false;
                    CrashProgressItem.IsRunning = true;
                    BackgroundColor = _applicationContext.Config.CrashChartWindowBackgroundColor;
                }
                else
                {
                    BackgroundColor = _applicationContext.Config.WindowBackgroundColor;
                }
            });

        public void Receive(ToggleChartWindowStateMessage _)
            => Dispatcher.UIThread.Invoke(() =>
            {
                if (_windowState == WindowState.FullScreen)
                {
                    WindowState = _previousWindowState;
                    return;
                }

                _previousWindowState = WindowState;

                WindowState = WindowState.FullScreen;
            });

        public void Receive(StartedMessage _)
            => Dispatcher.UIThread.Invoke(() =>
            {
                BackgroundColor = _applicationContext.Config.WindowBackgroundColor;

                PriceUpdateProgressItem.Duration = _applicationContext.Config.PriceUpdateInterval;
                PriceUpdateProgressItem.IsRunning = true;

                CrashProgressItem.IsRunning = true;
            });

        public void Receive(StoppedMessage _)
            => Dispatcher.UIThread.Invoke(() =>
            {
                PriceUpdateProgressItem.IsRunning = false;
                CrashProgressItem.IsRunning = false;
            });

        private void RefreshChart()
            => ChartItems = _applicationContext
            .Products
            .Select(x => ChartItem.From(x, _priceFormatHelper))
            .ToList();
    }
}
