using Stip.BattleGames.Common;

namespace Stip.Stipstonks;

public partial class App : AppBase
{
    public override void Initialize()
    {
        base.Initialize();
        _ = new Bootstrapper();
    }
}
