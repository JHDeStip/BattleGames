using System.Collections.Generic;

namespace Stip.Stipstonks.JsonModels
{
    public class Data
    {
        public int PriceUpdateIntervalInSeconds { get; set; }
        public int CrashIntervalInSeconds { get; set; }
        public int CrashDurationInSeconds { get; set; }
        public double MaxPriceDeviationFactor { get; set; }
        public int PriceResolutionInCents { get; set; }
        public bool AllowPriceUpdatesDuringOrder { get; set; }
        public string WindowBackgroundColor { get; set; }
        public string CrashChartWindowBackgroundColor { get; set; }
        public string PriceUpdateProgressBarColor { get; set; }
        public string CrashProgressBarColor { get; set; }
        public List<Product> Products { get; set; } = new();
    }
}
