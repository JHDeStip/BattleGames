using Caliburn.Micro;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Models;

namespace Stip.Stipstonks.Items
{
    public abstract class ProductItemBase : PropertyChangedBase
    {
        public PriceFormatHelper PriceFormatHelper { get; set; }

        public Product Product { get; set; }

        private string _name;
        public string Name { get => _name; set => Set(ref _name, value); }

        private string _color;
        public string Color { get => _color; set => Set(ref _color, value); }

        protected int _priceInCents;
        public virtual int PriceInCents
        {
            get => _priceInCents;
            set
            {
                if (Set(ref _priceInCents, value))
                {
                    NotifyOfPropertyChange(nameof(PriceString));
                }
            }
        }

        public string PriceString
            => PriceFormatHelper.Format(PriceInCents);
    }
}
