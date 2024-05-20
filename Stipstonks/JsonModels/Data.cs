using System.Collections.Generic;
using System.Linq;

namespace Stip.Stipstonks.JsonModels;

public record Data
{
    public required int PriceUpdateIntervalInSeconds { get; init; }
    public required int CrashIntervalInSeconds { get; init; }
    public required int CrashDurationInSeconds { get; init; }
    public required double MaxPriceDeviationFactor { get; init; }
    public required int PriceResolutionInCents { get; init; }
    public required bool AllowPriceUpdatesDuringOrder { get; init; }
    public required string WindowBackgroundColor { get; init; }
    public required string CrashChartWindowBackgroundColor { get; init; }
    public required string PriceUpdateProgressBarColor { get; init; }
    public required string CrashProgressBarColor { get; init; }
    public required IReadOnlyList<Product> Products { get; init; }

    public Models.Config ToConfig()
        => new()
        {
            PriceUpdateInterval = new(0, 0, PriceUpdateIntervalInSeconds),
            CrashInterval = new(0, 0, CrashIntervalInSeconds),
            CrashDuration = new(0, 0, CrashDurationInSeconds),
            MaxPriceDeviationFactor = MaxPriceDeviationFactor,
            PriceResolutionInCents = PriceResolutionInCents,
            AllowPriceUpdatesDuringOrder = AllowPriceUpdatesDuringOrder,
            WindowBackgroundColor = WindowBackgroundColor,
            CrashChartWindowBackgroundColor = CrashChartWindowBackgroundColor,
            PriceUpdateProgressBarColor = PriceUpdateProgressBarColor,
            CrashProgressBarColor = CrashProgressBarColor
        };

    public static Data From(Models.Config config, IEnumerable<Models.Product> products)
        => new()
        {
            PriceUpdateIntervalInSeconds = (int)config.PriceUpdateInterval.TotalSeconds,
            CrashIntervalInSeconds = (int)config.CrashInterval.TotalSeconds,
            CrashDurationInSeconds = (int)config.CrashDuration.TotalSeconds,
            MaxPriceDeviationFactor = config.MaxPriceDeviationFactor,
            PriceResolutionInCents = config.PriceResolutionInCents,
            AllowPriceUpdatesDuringOrder = config.AllowPriceUpdatesDuringOrder,
            WindowBackgroundColor = config.WindowBackgroundColor,
            CrashChartWindowBackgroundColor = config.CrashChartWindowBackgroundColor,
            PriceUpdateProgressBarColor = config.PriceUpdateProgressBarColor,
            CrashProgressBarColor = config.CrashProgressBarColor,
            Products = products.Select(Product.From).ToList()
        };
}
