using Caliburn.Micro;

namespace Stip.Stipstonks.Windows
{
    public abstract class ViewModelBase : Screen
    {
        public ViewModelBase()
            => DisplayName = UIStrings.Global_ApplicationName;
    }
}
