using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Stip.Stipstonks.Items;

public partial class StonkMarketEventProgressItem : ObservableObject
{
    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private TimeSpan _duration;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _color;
}
