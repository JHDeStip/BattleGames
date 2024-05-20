using Stip.BattleGames.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Helpers;

public class DelayHelper : IInjectable
{
    public virtual Task Delay(
        TimeSpan timeSpan,
        CancellationToken ct)
        => Task.Delay(
            timeSpan,
            ct);
}
