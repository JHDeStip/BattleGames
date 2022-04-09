using System;

namespace Stip.Stipstonks.Items
{
    public class InputItem : ProductItemBase
    {
        public Action TotalPriceChangedCallback;

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
            => PriceFormatHelper.Format(PriceInCents * Amount);

        public void Decrement()
        {
            if (Amount > 0)
            {
                --Amount;
            }
        }

        public void Increment()
            => ++Amount;
    }
}
