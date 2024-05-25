namespace Stip.BeerBattle.Models;

public record Group
{
    public required string Name { get; set; }
    public required string Color { get; set; }
    public required decimal TotalPoints { get; set; }
    public double Level { get; set; }
}
