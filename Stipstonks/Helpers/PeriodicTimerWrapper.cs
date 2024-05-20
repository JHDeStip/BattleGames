using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Helpers;

public class PeriodicTimerWrapper(TimeSpan period)
    : IPeriodicTimer
{
    private readonly PeriodicTimer _timer = new(period);

    public ValueTask<bool> WaitForNextTickAsync(CancellationToken ct = default)
        => _timer.WaitForNextTickAsync(ct);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _timer.Dispose();
    }
}
