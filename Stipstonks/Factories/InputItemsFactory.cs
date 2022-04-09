using AutoMapper;
using Caliburn.Micro;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Items;
using Stip.Stipstonks.Models;
using System.Collections.Generic;
using System.Linq;

namespace Stip.Stipstonks.Factories
{
    public class InputItemsFactory : IInjectable
    {
        public IMapper Mapper { get; set; }
        public PriceFormatHelper PriceFormatHelper { get; set; }

        public virtual List<InputItem> Create(
            IEnumerable<Product> products,
            IEnumerable<InputItem> existingInputItems,
            System.Action totalPriceChangedCallback)
        {
            var items = Mapper.Map<List<InputItem>>(products);

            items.ForEach(x =>
            {
                x.PriceFormatHelper = PriceFormatHelper;
                x.TotalPriceChangedCallback = totalPriceChangedCallback;
            });

            items
                .Zip(existingInputItems)
                .Apply(x => x.First.Amount = x.Second.Amount);

            return items;
        }
    }
}
