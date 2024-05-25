using Stip.BeerBattle.Helpers;
using Stip.BeerBattle.Models;

namespace Stip.BeerBattle.Items;

public class ChartItem
    (PointsFormatHelper _pointsFormatHelper)
    : GroupItemBase
{
    protected decimal _totalPoints;
    public virtual decimal TotalPoints
    {
        get => _totalPoints;
        set
        {
            if (SetProperty(ref _totalPoints, value))
            {
                OnPropertyChanged(nameof(TotalPointsString));
            }
        }
    }

    private double _level;
    public double Level { get => _level; set => SetProperty(ref _level, value); }

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
