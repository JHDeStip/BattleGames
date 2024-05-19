using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Models;

namespace Stip.Stipstonks.Items
{
    public class ChartItem
        (PriceFormatHelper _priceFormatHelper)
        : ProductItemBase(
            _priceFormatHelper)
    {
        private double _level;
        public double Level { get => _level; set => SetProperty(ref _level, value); }

        public static ChartItem From(
            Product product,
            PriceFormatHelper priceFormatHelper)
        {
            var item = new ChartItem(priceFormatHelper);
            item.UpdateWith(product);
            item.Level = product.Level;
            return item;
        }
    }
}
