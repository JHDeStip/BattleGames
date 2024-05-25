using Stip.BattleGames.Common.Windows;

namespace Stip.BeerBattle.Windows;

public partial class ChartWindowView : WindowBase<ChartWindowViewModel>
{
    public ChartWindowView(ChartWindowViewModel viewModel)
        : base(viewModel)
        => InitializeComponent();
}
