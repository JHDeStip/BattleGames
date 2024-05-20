using Stip.BattleGames.Common;
using Stip.Stipstonks.Factories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Helpers
{
    public abstract class StonkMarketEventManagerBase(
        PeriodicTimerFactory periodicTimerFactory)
        : IInjectable
    {
        protected readonly PeriodicTimerFactory _periodicTimerFactory = periodicTimerFactory;

        private CancellationTokenSource _cancellationTokenSource = new();
        private Task _task = Task.CompletedTask;

        public virtual Task StopAsync()
        {
            _cancellationTokenSource.Cancel();
            return _task;
        }

        protected void Start(TimeSpan timerInterval)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _task = InternalStartAsync(
                timerInterval);
        }

        private async Task InternalStartAsync(TimeSpan timerInterval)
        {
            var cancellationToken = _cancellationTokenSource.Token;

            using var timer = _periodicTimerFactory.Create(timerInterval);

            try
            {
                while (true)
                {
                    await timer.WaitForNextTickAsync(cancellationToken);
                    await OnTimerExpiredAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException) { }
        }

        protected abstract Task OnTimerExpiredAsync(CancellationToken ct);
    }
}
