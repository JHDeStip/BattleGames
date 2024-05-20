using Avalonia;
using System.Diagnostics;

namespace Stip.BeerBattle;

public class Program
{
    public static void Main()
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime([]);
        }
        catch
        {
#if DEBUG
            Debugger.Break();
#endif
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder
        .Configure<App>()
        .UsePlatformDetect()
        .WithInterFont();
}
