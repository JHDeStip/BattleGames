using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using System.Linq;

namespace Stip.BattleGames.Common
{
    public abstract class AppBase : Application
    {
        public Window GetLastOpenedWindow()
            => ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime).Windows.Last();
    }
}
