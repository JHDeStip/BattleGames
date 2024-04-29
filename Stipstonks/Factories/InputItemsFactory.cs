using Caliburn.Micro;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Items;
using Stip.Stipstonks.Models;
using System.Collections.Generic;
using System.Linq;

namespace Stip.Stipstonks.Factories
{
    public class InputItemsFactory(
        PriceFormatHelper _priceFormatHelper)
        : IInjectable
    {
        public virtual List<InputItem> Create(
            IEnumerable<Product> products,
            IEnumerable<InputItem> existingInputItems,
            System.Action totalPriceChangedCallback)
        {
            var items = products
                .Select(x => InputItem.From(
                    x,
                    _priceFormatHelper))
                .ToList();

            items.ForEach(x => { x.TotalPriceChangedCallback = totalPriceChangedCallback; });

            items
                .Zip(existingInputItems)
                .Apply(x => x.First.Amount = x.Second.Amount);

            return items;
        }
    }
}
