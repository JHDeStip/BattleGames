using CommunityToolkit.Mvvm.ComponentModel;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Models;

namespace Stip.Stipstonks.Items;

public partial class ChartItem
    (PriceFormatHelper _priceFormatHelper)
    : ProductItemBase(
        _priceFormatHelper)
{
    [ObservableProperty]
    private double _level;

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
