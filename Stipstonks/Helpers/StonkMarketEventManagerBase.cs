using Caliburn.Micro;
using Stip.Stipstonks.Factories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Helpers
{
    public abstract class StonkMarketEventManagerBase : IInjectable
    {
        public ApplicationContext ApplicationContext { get; set; }
        public PriceCalculator PriceRecalculator { get; set; }
        public IEventAggregator EventAggregator { get; set; }
        public PeriodicTimerFactory PeriodicTimerFactory { get; set; }

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

            using (var timer = PeriodicTimerFactory.Create(timerInterval))
            {
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
        }

        protected abstract Task OnTimerExpiredAsync(CancellationToken ct);
    }
}
