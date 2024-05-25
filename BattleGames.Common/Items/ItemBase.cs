using CommunityToolkit.Mvvm.ComponentModel;

namespace Stip.BattleGames.Common.Items;

public abstract class ItemBase : ObservableObject
{
    private string _name;
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    private string _color;
    public string Color { get => _color; set => SetProperty(ref _color, value); }
}
