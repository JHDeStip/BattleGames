using Caliburn.Micro;
using System.Windows;

namespace Stip.Stipstonks.Items
{
    public class StonkMarketEventProgressItem : PropertyChangedBase
    {
        private Duration _duration;
        public Duration Duration { get => _duration; set => Set(ref _duration, value); }

        private bool _isRunning;
        public bool IsRunning { get => _isRunning; set => Set(ref _isRunning, value); }

        private string _color;
        public string Color { get => _color; set => Set(ref _color, value); }

    }
}
