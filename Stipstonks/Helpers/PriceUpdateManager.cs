using CommunityToolkit.Mvvm.Messaging;
using Stip.Stipstonks.Factories;
using Stip.Stipstonks.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Helpers
{
    public class PriceUpdateManager(
        ApplicationContext _applicationContext,
        IMessenger _messenger,
        PriceCalculator _priceCalculator,
        PeriodicTimerFactory periodicTimerFactory)
        : StonkMarketEventManagerBase(
            periodicTimerFactory)
    {
        public virtual void Start()
            => Start(_applicationContext.Config.PriceUpdateInterval);

        protected override Task OnTimerExpiredAsync(CancellationToken ct)
        {
            _periodicTimerFactory.ToString();
            _priceCalculator.RecalculatePrices(
                _applicationContext.Products,
                _applicationContext.Config.MaxPriceDeviationFactor,
                _applicationContext.Config.PriceResolutionInCents);

            _messenger.Send<PricesUpdatedMessage>();

            return Task.CompletedTask;
        }
    }
}
