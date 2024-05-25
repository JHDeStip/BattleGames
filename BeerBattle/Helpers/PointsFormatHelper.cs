using Stip.BattleGames.Common;
using Stip.BeerBattle.Models;

namespace Stip.BeerBattle.Helpers;

public class PointsFormatHelper : IInjectable
{
    private string _pointsFormat = string.Empty;

    public virtual void Initialize(Config config)
        => _pointsFormat = "0." + "".PadRight(config.TotalPointsNumberOfDecimals, '0');

    public virtual string Format(decimal points)
        => points.ToString(_pointsFormat);
}
