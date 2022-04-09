namespace Stip.Stipstonks.Models
{
    public class Product
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public int BasePriceInCents { get; set; }
        public int TotalAmountSold { get; set; }
        public int VirtualAmountSold { get; set; }
        public int CurrentPriceInCents { get; set; }
        public double Level { get; set; }
    }
}
