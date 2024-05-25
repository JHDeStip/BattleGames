namespace Stip.BeerBattle.Models;

public record Product
{
    public required string Name { get; set; }
    public required string Color { get; set; }
    public required decimal PointsPerItem { get; set; }
}
