using Stip.BattleGames.Common;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Items;
using Stip.Stipstonks.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stip.Stipstonks.Factories;

public class InputItemsFactory(
    PriceFormatHelper _priceFormatHelper)
    : IInjectable
{
    public virtual List<InputItem> Create(
        IEnumerable<Product> products,
        IEnumerable<InputItem> existingInputItems,
        Action totalPriceChangedCallback)
    {
        var items = products
            .Select(x => InputItem.From(
                x,
                _priceFormatHelper))
            .ToList();

        items.ForEach(x => { x.TotalPriceChangedCallback = totalPriceChangedCallback; });

        foreach (var (first, second) in items.Zip(existingInputItems))
        {
            first.Amount = second.Amount;
        }

        return items;
    }
}
