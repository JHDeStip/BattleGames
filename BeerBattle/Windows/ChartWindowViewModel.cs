﻿using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Stip.BattleGames.Common.Windows;
using Stip.BeerBattle.Helpers;
using Stip.BeerBattle.Items;
using Stip.BeerBattle.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.BeerBattle.Windows;

public partial class ChartWindowViewModel(
    ApplicationContext _applicationContext,
    IMessenger _messenger,
    PointsFormatHelper _pointsFormatHelper)
    : ChartWindowViewModelBase(_messenger),
    IRecipient<PointsUpdatedMessage>
{
    [ObservableProperty]
    private IReadOnlyList<ChartItem> _chartItems = [];

    public override async ValueTask InitializeAsync(CancellationToken ct)
    {
        await base.InitializeAsync(ct);
        BackgroundColor = _applicationContext.Config.WindowBackgroundColor;
    }

    public override async ValueTask ActivateAsync(CancellationToken ct)
    {
        await base.ActivateAsync(ct);
        _messenger.Register<PointsUpdatedMessage>(this);
        RefreshChart();
    }

    public void Receive(PointsUpdatedMessage _)
        => Dispatcher.UIThread.Invoke(RefreshChart);

    private void RefreshChart()
        => ChartItems = _applicationContext
        .Groups
        .Select(x => ChartItem.From(x, _pointsFormatHelper))
        .ToList();
}
