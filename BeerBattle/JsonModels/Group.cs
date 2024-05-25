namespace Stip.BeerBattle.JsonModels;

public record Group
{
    public required string Name { get; init; }
    public required string Color { get; init; }
    public required decimal TotalPoints { get; init; }

    public Models.Group ToModel()
        => new()
        {
            Name = Name,
            Color = Color,
            TotalPoints = TotalPoints
        };

    public static Group From(Models.Group group)
        => new()
        {
            Name = group.Name,
            Color = group.Color,
            TotalPoints = group.TotalPoints
        };
}
