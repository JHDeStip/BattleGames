using Avalonia;
using Stip.BattleGames.Common.Helpers;
using System;
using System.Diagnostics;
using System.IO;

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
        catch (Exception e)
        {
#if DEBUG
            Debugger.Break();
#endif
            File.WriteAllText(
                Path.Combine(new EnvironmentHelper().ExecutableDirectory, $"{UIStrings.Global_ApplicationName}.Crash.log"),
                $"{e.Message}{Environment.NewLine}{e.StackTrace}");
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder
        .Configure<App>()
        .UsePlatformDetect()
        .WithInterFont();
}
