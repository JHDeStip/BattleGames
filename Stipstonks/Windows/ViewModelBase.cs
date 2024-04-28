using Caliburn.Micro;
using CommunityToolkit.Mvvm.Messaging;
using Stip.Stipstonks.Helpers;

namespace Stip.Stipstonks.Windows
{
    public abstract class ViewModelBase : Screen
    {
        public ApplicationContext ApplicationContext { get; set; }
        public IMessenger Messenger { get; set; }
        public PriceFormatHelper PriceFormatHelper { get; set; }

        public ViewModelBase()
            => DisplayName = UIStrings.Global_ApplicationName;
    }
}
