using Stip.BattleGames.Common;
using System;
using System.Globalization;

namespace Stip.Stipstonks.Helpers
{
    public class PriceFormatHelper : IInjectable
    {
        public virtual string Format(int priceInCents)
            => $"{CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol}{priceInCents / 100}{CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator}{string.Format("{0:00}", Math.Abs(priceInCents % 100))}";
    }
}
