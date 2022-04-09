namespace Stip.Stipstonks.JsonModels
{
    public class Product
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public int BasePriceInCents { get; set; }
        public int TotalAmountSold { get; set; }
        public int VirtualAmountSold { get; set; }
    }
}
