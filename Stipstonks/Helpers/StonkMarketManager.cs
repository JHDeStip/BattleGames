using Stip.BattleGames.Common;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Helpers
{
    public class StonkMarketManager(
        PriceUpdateManager _priceUpdateManager,
        CrashManager _crashManager)
        : IInjectable
    {
        private bool _isRunning;

        public virtual void Start()
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;

            var startPriceUpdateManager = _priceUpdateManager.Start;

            startPriceUpdateManager();

            _crashManager.Start(
                _priceUpdateManager.StopAsync,
                startPriceUpdateManager);
        }

        public virtual async Task StopAsync()
        {
            if (!_isRunning)
            {
                return;
            }

            await Task
                .WhenAll(
                    _priceUpdateManager.StopAsync(),
                    _crashManager.StopAsync())
                .ConfigureAwait(false);

            _isRunning = false;
        }
    }
}
