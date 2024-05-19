namespace Stip.Stipstonks.Windows
{
    public partial class ChartWindowView : WindowBase<ChartWindowViewModel>
    {
        public ChartWindowView(ChartWindowViewModel viewModel)
            : base(viewModel)
            => InitializeComponent();
    }
}
