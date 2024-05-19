using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Stip.Stipstonks.Items
{
    public class StonkMarketEventProgressItem : ObservableObject
    {
        private TimeSpan _duration;
        public TimeSpan Duration { get => _duration; set => SetProperty(ref _duration, value); }

        private bool _isRunning;
        public bool IsRunning { get => _isRunning; set => SetProperty(ref _isRunning, value); }

        private string _color;
        public string Color { get => _color; set => SetProperty(ref _color, value); }
    }
}
