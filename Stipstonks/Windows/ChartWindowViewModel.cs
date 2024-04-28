using CommunityToolkit.Mvvm.Messaging;
using Stip.Stipstonks.Items;
using Stip.Stipstonks.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Stip.Stipstonks.Windows
{
    public class ChartWindowViewModel
        : ViewModelBase,
        IRecipient<PricesUpdatedMessage>,
        IRecipient<ToggleChartWindowStateMessage>,
        IRecipient<StartedMessage>,
        IRecipient<StoppedMessage>
    {
        public StonkMarketEventProgressItem PriceUpdateProgressItem { get; set; } = new();
        public StonkMarketEventProgressItem CrashProgressItem { get; set; } = new();

        private IReadOnlyList<ChartItem> _chartItems = new List<ChartItem>();
        public IReadOnlyList<ChartItem> ChartItems { get => _chartItems; set => Set(ref _chartItems, value); }

        private string _backgroundColor;
        public string BackgroundColor { get => _backgroundColor; set => Set(ref _backgroundColor, value); }

        private WindowStyle _windowStyle = WindowStyle.SingleBorderWindow;
        public WindowStyle WindowStyle { get => _windowStyle; set => Set(ref _windowStyle, value); }

        private WindowState _windowState;
        public WindowState WindowState { get => _windowState; set => Set(ref _windowState, value); }
        
        private WindowStyle _previousWindowStyle;
        private WindowState _previousWindowState;
        private bool canClose;

        protected override async Task OnInitializeAsync(CancellationToken ct)
        {
            await base.OnInitializeAsync(ct);

            BackgroundColor = ApplicationContext.Config.WindowBackgroundColor;

            PriceUpdateProgressItem.Color = ApplicationContext.Config.PriceUpdateProgressBarColor;

            CrashProgressItem.Color = ApplicationContext.Config.CrashProgressBarColor;
            CrashProgressItem.Duration = new(ApplicationContext.Config.CrashInterval);
        }

        protected override async Task OnActivateAsync(CancellationToken ct)
        {
            await base.OnActivateAsync(ct);

            Messenger.RegisterAll(this);
            RefreshChart();
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken ct)
        {
            await base.OnDeactivateAsync(
                close,
                ct);

            Messenger.UnregisterAll(this);
        }

        public override async Task<bool> CanCloseAsync(CancellationToken ct)
            => canClose
            && await base.CanCloseAsync(ct);

        public override Task TryCloseAsync(bool? dialogResult = null)
        {
            canClose = true;
            return base.TryCloseAsync(dialogResult);
        }

        public void Receive(PricesUpdatedMessage _)
            => OnUIThread(() =>
            {
                RefreshChart();

                PriceUpdateProgressItem.IsRunning = false;

                PriceUpdateProgressItem.Duration = new Duration(ApplicationContext.HasCrashed
                    ? ApplicationContext.Config.CrashDuration
                    : ApplicationContext.Config.PriceUpdateInterval);

                PriceUpdateProgressItem.IsRunning = true;

                if (ApplicationContext.HasCrashed)
                {
                    CrashProgressItem.IsRunning = false;
                    CrashProgressItem.IsRunning = true;
                    BackgroundColor = ApplicationContext.Config.CrashChartWindowBackgroundColor;
                }
                else
                {
                    BackgroundColor = ApplicationContext.Config.WindowBackgroundColor;
                }
            });

        public void Receive(ToggleChartWindowStateMessage _)
            => OnUIThread(() =>
            {
                if (_windowStyle == WindowStyle.None)
                {
                    WindowStyle = _previousWindowStyle;
                    WindowState = _previousWindowState;
                    return;
                }

                _previousWindowStyle = WindowStyle;
                _previousWindowState = WindowState;

                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            });

        public void Receive(StartedMessage _)
            => OnUIThread(() =>
            {
                BackgroundColor = ApplicationContext.Config.WindowBackgroundColor;

                PriceUpdateProgressItem.Duration = new(ApplicationContext.Config.PriceUpdateInterval);
                PriceUpdateProgressItem.IsRunning = true;

                CrashProgressItem.IsRunning = true;
            });

        public void Receive(StoppedMessage _)
            => OnUIThread(() =>
            {
                PriceUpdateProgressItem.IsRunning = false;
                CrashProgressItem.IsRunning = false;
            });

        private void RefreshChart()
        {
            var chartItems = ApplicationContext.Products.Select(ChartItem.From).ToList();
            chartItems.ForEach(x => x.PriceFormatHelper = PriceFormatHelper);
            ChartItems = chartItems;
        }
    }
}
