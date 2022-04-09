using System;

namespace Stip.Stipstonks.Models
{
    public class Config
    {
        public TimeSpan PriceUpdateInterval { get; set; }
        public TimeSpan CrashInterval { get; set; }
        public TimeSpan CrashDuration { get; set; }
        public double MaxPriceDeviationFactor { get; set; }
        public int PriceResolutionInCents { get; set; }
        public bool AllowPriceUpdatesDuringOrder { get; set; }
        public string WindowBackgroundColor { get; set; }
        public string CrashChartWindowBackgroundColor { get; set; }
        public string PriceUpdateProgressBarColor { get; set; }
        public string CrashProgressBarColor { get; set; }
    }
}
