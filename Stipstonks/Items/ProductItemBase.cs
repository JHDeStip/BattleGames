using CommunityToolkit.Mvvm.ComponentModel;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Models;

namespace Stip.Stipstonks.Items
{
    public abstract class ProductItemBase(
        PriceFormatHelper priceFormatHelper)
        : ObservableObject
    {
        public Product Product { get; set; }

        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private string _color;
        public string Color { get => _color; set => SetProperty(ref _color, value); }

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

        protected readonly PriceFormatHelper _priceFormatHelper = priceFormatHelper;

        protected void UpdateWith(
            Product product)
        {
            Product = product;
            Name = product.Name;
            Color = product.Color;
            PriceInCents = product.CurrentPriceInCents;
        }
    }
}
