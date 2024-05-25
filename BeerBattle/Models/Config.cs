namespace Stip.BeerBattle.Models;

public record Config
{
    public required string WindowBackgroundColor { get; init; }
    public required int TotalPointsNumberOfDecimals { get; init; }
}
