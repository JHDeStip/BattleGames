using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using System.Linq;

namespace Stip.BattleGames.Common;

public abstract class AppBase : Application
{
    public Window GetActiveWindow()
    {
        var allWindows = ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime).Windows;
        return allWindows.FirstOrDefault(x => x.IsActive) ?? allWindows.First();
    }
}
