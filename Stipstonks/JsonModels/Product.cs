namespace Stip.Stipstonks.JsonModels
{
    public record Product
    {
        public required string Name { get; init; }
        public required string Color { get; init; }
        public required int BasePriceInCents { get; init; }
        public required int TotalAmountSold { get; init; }
        public required int VirtualAmountSold { get; init; }

        public Models.Product ToModel()
            => new()
            {
                Name = Name,
                Color = Color,
                BasePriceInCents = BasePriceInCents,
                TotalAmountSold = TotalAmountSold,
                VirtualAmountSold = VirtualAmountSold
            };

        public static Product From(Models.Product product)
            => new()
            {
                Name = product.Name,
                Color = product.Color,
                BasePriceInCents = product.BasePriceInCents,
                TotalAmountSold = product.TotalAmountSold,
                VirtualAmountSold = product.VirtualAmountSold
            };
    }
}
