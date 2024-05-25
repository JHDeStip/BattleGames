using Stip.BattleGames.Common.Windows;

namespace Stip.BeerBattle.Windows;

public partial class InputWindowView : WindowBase<InputWindowViewModel>
{
    public InputWindowView(InputWindowViewModel viewModel)
        : base(viewModel)
        => InitializeComponent();
}
