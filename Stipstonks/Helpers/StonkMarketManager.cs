using System.Threading.Tasks;

namespace Stip.Stipstonks.Helpers
{
    public class StonkMarketManager : IInjectable
    {
        public PriceUpdateManager PriceUpdateManager { get; set; }
        public CrashManager CrashManager { get; set; }

        private bool _isRunning;

        public virtual void Start()
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;

            var startPriceUpdateManager = () => PriceUpdateManager.Start();

            startPriceUpdateManager();

            CrashManager.Start(
                () => PriceUpdateManager.StopAsync(),
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
                    PriceUpdateManager.StopAsync(),
                    CrashManager.StopAsync())
                .ConfigureAwait(false);

            _isRunning = false;
        }
    }
}
