using System;

namespace Stip.Stipstonks.Models;

public record Config
{
    public required TimeSpan PriceUpdateInterval { get; init; }
    public required TimeSpan CrashInterval { get; init; }
    public required TimeSpan CrashDuration { get; init; }
    public required double MaxPriceDeviationFactor { get; init; }
    public required int PriceResolutionInCents { get; init; }
    public required bool AllowPriceUpdatesDuringOrder { get; init; }
    public required string WindowBackgroundColor { get; init; }
    public required string CrashChartWindowBackgroundColor { get; init; }
    public required string PriceUpdateProgressBarColor { get; init; }
    public required string CrashProgressBarColor { get; init; }
    public required bool ShowCrashProgressBar { get; init; }
}
