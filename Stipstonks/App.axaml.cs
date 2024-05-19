using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.Linq;

namespace Stip.Stipstonks
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            base.Initialize();
            new Bootstrapper();
        }

        public Window GetLastOpenedWindow()
            => ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime).Windows.Last();
    }
}
