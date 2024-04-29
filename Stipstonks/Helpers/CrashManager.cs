using CommunityToolkit.Mvvm.Messaging;
using Stip.Stipstonks.Factories;
using Stip.Stipstonks.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Helpers
{
    public class CrashManager(
        ApplicationContext _applicationContext,
        IMessenger _messenger,
        PriceCalculator _priceCalculator,
        DataPersistenceHelper _dataPersistenceHelper,
        DelayHelper _delayHelper,
        PeriodicTimerFactory periodicTimerFactory)
        : StonkMarketEventManagerBase(
            periodicTimerFactory)
    {
        private Func<Task> _stonkMarketWillCrashAction;
        private Action _stonkMarketCrashEndedAction;

        public virtual void Start(
            Func<Task> stonkMarketWillCrashAction,
            Action stonkMarketCrashEndedAction)
        {
            _stonkMarketWillCrashAction = stonkMarketWillCrashAction;
            _stonkMarketCrashEndedAction = stonkMarketCrashEndedAction;

            Start(_applicationContext.Config.CrashInterval);
        }

        protected override async Task OnTimerExpiredAsync(CancellationToken ct)
        {
            await _stonkMarketWillCrashAction();

            _applicationContext.HasCrashed = true;

            _priceCalculator.Crash(
                _applicationContext.Products,
                _applicationContext.Config.MaxPriceDeviationFactor,
                _applicationContext.Config.PriceResolutionInCents);

            try
            {
                _messenger.Send<PricesUpdatedMessage>();

                await _dataPersistenceHelper.SaveDataAsync();

                await _delayHelper.Delay(
                    _applicationContext.Config.CrashDuration,
                    ct);

                _priceCalculator.ResetPricesAfterCrash(_applicationContext.Products);

                _applicationContext.HasCrashed = false;

                _messenger.Send<PricesUpdatedMessage>();

                _stonkMarketCrashEndedAction();
            }
            finally
            {
                _applicationContext.HasCrashed = false;
            }
        }
    }
}
