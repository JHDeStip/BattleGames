using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Helpers;

public interface IPeriodicTimer : IDisposable
{
    ValueTask<bool> WaitForNextTickAsync(CancellationToken ct = default);
}
