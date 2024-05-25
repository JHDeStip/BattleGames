namespace Stip.BeerBattle.JsonModels;

public record Product
{
    public required string Name { get; init; }
    public required string Color { get; init; }
    public required decimal PointsPerItem { get; init; }

    public Models.Product ToModel()
        => new()
        {
            Name = Name,
            Color = Color,
            PointsPerItem = PointsPerItem
        };

    public static Product From(Models.Product product)
        => new()
        {
            Name = product.Name,
            Color = product.Color,
            PointsPerItem = product.PointsPerItem
        };
}
