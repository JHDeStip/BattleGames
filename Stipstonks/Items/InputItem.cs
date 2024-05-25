﻿using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Models;
using System;

namespace Stip.Stipstonks.Items;

public class InputItem
    (PriceFormatHelper _priceFormatHelper)
    : ProductItemBase(
        _priceFormatHelper)
{
    public Action TotalPriceChangedCallback { get; set; }

    private int _amount;
    public int Amount
    {
        get => _amount;
        set
        {
            if (SetProperty(ref _amount, value))
            {
                TotalPriceChangedCallback?.Invoke();
            }
        }
    }

    public override int PriceInCents
    {
        get => _priceInCents;
        set
        {
            if (SetProperty(ref _priceInCents, value))
            {
                OnPropertyChanged(nameof(PriceString));
                TotalPriceChangedCallback?.Invoke();
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

    public static InputItem From(
        Product product,
        PriceFormatHelper priceFormatHelper)
    {
        var item = new InputItem(priceFormatHelper);
        item.UpdateWith(product);
        return item;
    }
}
