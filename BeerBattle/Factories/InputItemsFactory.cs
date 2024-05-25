using Stip.BattleGames.Common;
using Stip.BeerBattle.Items;
using Stip.BeerBattle.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stip.BeerBattle.Factories;

public class InputItemsFactory() : IInjectable
{
    public virtual List<InputItem> Create(
        IEnumerable<Product> products,
        Action totalPriceChangedCallback)
    {
        var items = products.Select(InputItem.From).ToList();

        items.ForEach(x => { x.TotalPointsChangedCallback = totalPriceChangedCallback; });

        return items;
    }
}
