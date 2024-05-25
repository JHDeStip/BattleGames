using Stip.BattleGames.Common.Items;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Models;

namespace Stip.Stipstonks.Items;

public abstract class ProductItemBase(
    PriceFormatHelper _priceFormatHelper)
    : ItemBase
{
    public Product Product { get; set; }

    protected int _priceInCents;
    public virtual int PriceInCents
    {
        get => _priceInCents;
        set
        {
            if (SetProperty(ref _priceInCents, value))
            {
                OnPropertyChanged(nameof(PriceString));
            }
        }
    }

    public string PriceString
        => _priceFormatHelper.Format(PriceInCents);

    protected void UpdateWith(
        Product product)
    {
        Product = product;
        Name = product.Name;
        Color = product.Color;
        PriceInCents = product.CurrentPriceInCents;
    }
}
