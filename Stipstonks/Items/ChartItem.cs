namespace Stip.Stipstonks.Items
{
    public class ChartItem : ProductItemBase
    {
        private double _level;
        public double Level { get => _level; set => Set(ref _level, value); }
    }
}
