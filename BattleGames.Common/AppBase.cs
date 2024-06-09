using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using System.Linq;
using Avalonia.Styling;

namespace Stip.BattleGames.Common;

public abstract class AppBase : Application
{
    public override void Initialize()
    {
        base.Initialize();
        RequestedThemeVariant = ThemeVariant.Light;
    }

    public Window GetActiveWindow()
    {
        var lifetime = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        return lifetime?.Windows.FirstOrDefault(x => x.IsActive) ?? lifetime?.Windows.FirstOrDefault();
    }
}
