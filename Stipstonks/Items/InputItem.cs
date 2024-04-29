using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Models;
using System;

namespace Stip.Stipstonks.Items
{
    public class InputItem
        (PriceFormatHelper priceFormatHelper)
        : ProductItemBase(
            priceFormatHelper)
    {
        public Action TotalPriceChangedCallback { get; set; }

        private int _amount;
        public int Amount
        {
            get => _amount;
            set
            {
                if (Set(ref _amount, value))
                {
                    NotifyOfPropertyChange(nameof(TotalPriceString));
                    TotalPriceChangedCallback?.Invoke();
                }
            }
        }

        public override int PriceInCents
        {
            get => _priceInCents;
            set
            {
                if (Set(ref _priceInCents, value))
                {
                    NotifyOfPropertyChange(nameof(PriceString));
                    NotifyOfPropertyChange(nameof(TotalPriceString));
                    TotalPriceChangedCallback?.Invoke();
                }
            }
        }

        public string TotalPriceString
            => _priceFormatHelper.Format(PriceInCents * Amount);

        public void Decrement()
        {
            if (Amount > 0)
            {
                --Amount;
            }
        }

        public void Increment()
            => ++Amount;

        public static InputItem From(
            Product product,
            PriceFormatHelper priceFormatHelper)
        {
            var item = new InputItem(priceFormatHelper);
            item.UpdateWith(product);
            return item;
        }
    }
}
