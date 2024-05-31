using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Stip.BattleGames.Common.Windows;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Items;
using Stip.Stipstonks.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Windows;

public partial class ChartWindowViewModel(
    ApplicationContext _applicationContext,
    IMessenger _messenger,
    PriceFormatHelper _priceFormatHelper)
    : ChartWindowViewModelBase(_messenger),
    IRecipient<PricesUpdatedMessage>,
    IRecipient<StartedMessage>,
    IRecipient<StoppedMessage>
{
    public StonkMarketEventProgressItem PriceUpdateProgressItem { get; set; } = new();
    public StonkMarketEventProgressItem CrashProgressItem { get; set; } = new();

    [ObservableProperty]
    private IReadOnlyList<ChartItem> _chartItems = [];

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
        RefreshChart();
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
