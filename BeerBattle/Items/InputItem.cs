using Stip.BattleGames.Common.Items;
using Stip.BeerBattle.Models;
using System;

namespace Stip.BeerBattle.Items;

public class InputItem : ItemBase
{
    public required Product Product { get; init; }
    public Action TotalPointsChangedCallback { get; set; }

    private int _amount;
    public int Amount
    {
        get => _amount;
        set
        {
            if (SetProperty(ref _amount, value))
            {
                TotalPointsChangedCallback?.Invoke();
            }
        }
    }

    public void Decrement()
    {
        if (Amount > 0)
        {
            --Amount;
        }
    }

    public void Increment()
        => ++Amount;

    public static InputItem From(Product product)
        => new()
        {
            Product = product,
            Name = product.Name,
            Color = product.Color,
        };
}
