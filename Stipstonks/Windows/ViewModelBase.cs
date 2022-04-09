using AutoMapper;
using Caliburn.Micro;
using Stip.Stipstonks.Helpers;

namespace Stip.Stipstonks.Windows
{
    public abstract class ViewModelBase : Screen
    {
        public ApplicationContext ApplicationContext { get; set; }
        public IEventAggregator EventAggregator { get; set; }
        public IMapper Mapper { get; set; }
        public PriceFormatHelper PriceFormatHelper { get; set; }

        public ViewModelBase()
            => DisplayName = UIStrings.Global_ApplicationName;
    }
}
