using CommunityToolkit.Mvvm.Messaging;
using Stip.Stipstonks.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Helpers
{
    public class PriceUpdateManager : StonkMarketEventManagerBase
    {
        public virtual void Start()
            => Start(ApplicationContext.Config.PriceUpdateInterval);

        protected override Task OnTimerExpiredAsync(CancellationToken ct)
        {
            PriceRecalculator.RecalculatePrices(
                ApplicationContext.Products,
                ApplicationContext.Config.MaxPriceDeviationFactor,
                ApplicationContext.Config.PriceResolutionInCents);

            Messenger.Send<PricesUpdatedMessage>();

            return Task.CompletedTask;
        }
    }
}
