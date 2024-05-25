using Avalonia;
using Stip.BattleGames.Common.Windows;

namespace Stip.Stipstonks.Windows;

public partial class InputWindowView : WindowBase<InputWindowViewModel>
{
    private Vector _lastScrollPosition = Vector.Zero;

    public InputWindowView(InputWindowViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();

        InputItemsScrollViewer.ScrollChanged += (_, _) => _lastScrollPosition = InputItemsScrollViewer.Offset;
        InputItemsItemsControl.SizeChanged += (_, _) => InputItemsScrollViewer.Offset = _lastScrollPosition;
    }
}
