using Stip.Stipstonks.Models;

namespace Stip.Stipstonks.Items
{
    public class ChartItem : ProductItemBase
    {
        private double _level;
        public double Level { get => _level; set => Set(ref _level, value); }

        public static ChartItem From(Product product)
        {
            var item = From<ChartItem>(product);
            item.Level = product.Level;
            return item;
        }
    }
}
