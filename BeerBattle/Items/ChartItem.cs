using CommunityToolkit.Mvvm.ComponentModel;
using Stip.BeerBattle.Helpers;
using Stip.BeerBattle.Models;

namespace Stip.BeerBattle.Items;

public partial class ChartItem
    (PointsFormatHelper _pointsFormatHelper)
    : GroupItemBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPointsString))]
    protected decimal _totalPoints;

    [ObservableProperty]
    private double _level;

    public string TotalPointsString
        => _pointsFormatHelper.Format(TotalPoints);

    public static ChartItem From(
        Group group,
        PointsFormatHelper pointsFormatHelper)
    {
        var item = new ChartItem(pointsFormatHelper);
        item.UpdateWith(group);
        item.TotalPoints = group.TotalPoints;
        item.Level = group.Level;
        return item;
    }
}
