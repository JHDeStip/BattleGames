using CommunityToolkit.Mvvm.ComponentModel;
using Stip.BattleGames.Common.Items;
using Stip.BeerBattle.Models;
using System;

namespace Stip.BeerBattle.Items;

public partial class InputItem : ItemBase
{
    public required Product Product { get; init; }
    public Action TotalPointsChangedCallback { get; set; }

    [ObservableProperty]
    private int _amount;

    partial void OnAmountChanged(int value)
        => TotalPointsChangedCallback?.Invoke();

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
