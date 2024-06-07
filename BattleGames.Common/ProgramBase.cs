using Stip.BattleGames.Common.Helpers;
using System.Diagnostics;
using System.Reflection;
using System;
using System.IO;
using Avalonia;

namespace Stip.BattleGames.Common;

public abstract class ProgramBase
{
    protected static void StartApp<T>() where T : Application, new()
    {
        try
        {
            BuildAvaloniaApp<T>()
                .StartWithClassicDesktopLifetime([]);
        }
        catch (Exception e)
        {
#if DEBUG
            Debugger.Break();
#endif
            File.WriteAllText(
                Path.Combine(new EnvironmentHelper().ExecutableDirectory, $"{Assembly.GetEntryAssembly()?.GetName().Name}.Crash.log"),
                $"{e.Message}{Environment.NewLine}{e.StackTrace}");
        }
    }

    protected static AppBuilder BuildAvaloniaApp<T>() where T : Application, new()
        => AppBuilder
        .Configure<T>()
        .UsePlatformDetect()
        .WithInterFont();
}
