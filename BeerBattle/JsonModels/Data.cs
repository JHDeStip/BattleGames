using System.Collections.Generic;
using System.Linq;

namespace Stip.BeerBattle.JsonModels;

public record Data
{
    public required string WindowBackgroundColor { get; init; }
    public required int TotalPointsNumberOfDecimals { get; init; }
    public required IReadOnlyList<Group> Groups { get; init; }
    public required IReadOnlyList<Product> Products { get; init; }

    public Models.Config ToConfig()
        => new()
        {
            WindowBackgroundColor = WindowBackgroundColor,
            TotalPointsNumberOfDecimals = TotalPointsNumberOfDecimals
        };

    public static Data From(
        Models.Config config,
        IEnumerable<Models.Group> groups,
        IEnumerable<Models.Product> products)
        => new ()
        {
            WindowBackgroundColor = config.WindowBackgroundColor,
            TotalPointsNumberOfDecimals = config.TotalPointsNumberOfDecimals,
            Groups = groups.Select(Group.From).ToList(),
            Products = products.Select(Product.From).ToList()
        };
}
