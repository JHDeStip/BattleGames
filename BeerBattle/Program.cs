using Avalonia;
using Stip.BattleGames.Common;

namespace Stip.BeerBattle;

public class Program : ProgramBase
{
    public static void Main()
        => StartApp<App>();

    public static AppBuilder BuildAvaloniaApp()
        => BuildAvaloniaApp<App>();
}
