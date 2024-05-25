using Avalonia;
using Stip.BattleGames.Common.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Stip.Stipstonks;

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
                Path.Combine(new EnvironmentHelper().ExecutableDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.Crash.log"),
                $"{e.Message}{Environment.NewLine}{e.StackTrace}");
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder
        .Configure<App>()
        .UsePlatformDetect()
        .WithInterFont();
}
