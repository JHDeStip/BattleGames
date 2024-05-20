namespace Stip.Stipstonks.Models;

public record Product
{
    public required string Name { get; set; }
    public required string Color { get; set; }
    public required int BasePriceInCents { get; set; }
    public required int TotalAmountSold { get; set; }
    public required int VirtualAmountSold { get; set; }
    public int CurrentPriceInCents { get; set; }
    public double Level { get; set; }
}
