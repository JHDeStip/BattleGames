using CommunityToolkit.Mvvm.ComponentModel;

namespace Stip.BattleGames.Common.Items;

public abstract partial class ItemBase : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _color;
}
