namespace Stip.Stipstonks.Windows
{
    public partial class InputWindowView : WindowBase<InputWindowViewModel>
    {
        public InputWindowView(InputWindowViewModel viewModel)
            : base(viewModel)
            => InitializeComponent();
    }
}
