using Caliburn.Micro;
using Castle.Windsor;
using Stip.Stipstonks.Extensions;
using Stip.Stipstonks.Factories;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Items;
using Stip.Stipstonks.Messages;
using Stip.Stipstonks.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Windows
{
    public class InputWindowViewModel
        : ViewModelBase,
        IUIEnabled,
        IHandle<PricesUpdatedMessage>
    {
        public DataPersistenceHelper DataPersistenceHelper { get; set; }
        public StonkMarketManager StonkMarketManager { get; set; }
        public PriceCalculator PriceCalculator { get; set; }
        public DisableUIService DisableUIService { get; set; }
        public DialogService DialogService { get; set; }
        public IWindsorContainer Container { get; set; }
        public InputItemsFactory InputItemsFactory { get; set; }

        public string BackgroundColor { get; private set; }

        private bool _uiEnabled = true;
        public bool UIEnabled { get => _uiEnabled; set => Set(ref _uiEnabled, value); }

        private List<InputItem> _inputItems = new();
        public List<InputItem> InputItems { get => _inputItems; set => Set(ref _inputItems, value); }

        private string _totalPriceString;
        public string TotalPriceString { get => _totalPriceString; private set => Set(ref _totalPriceString, value); }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (Set(ref _isRunning, value))
                {
                    NotifyOfPropertyChange(nameof(IsNotRunning));
                }
            }
        }

        public bool IsNotRunning => !IsRunning;

        public InputWindowViewModel()
            => DisplayName = UIStrings.Global_ApplicationName;

        protected override async Task OnInitializeAsync(CancellationToken ct)
        {
            await base.OnInitializeAsync(ct);

            BackgroundColor = ApplicationContext.Config.WindowBackgroundColor;
        }

        protected override async Task OnActivateAsync(CancellationToken ct)
        {
            await base.OnActivateAsync(ct);

            EventAggregator.SubscribeOnUIThread(this);
            UpdateItems();
        }

        protected override async Task OnDeactivateAsync(
            bool close,
            CancellationToken ct)
        {
            EventAggregator.Unsubscribe(this);

            if (close)
            {
                using (Container.ResolveComponent<ChartWindowViewModel>(out var chartWindowViewModel))
                {
                    await chartWindowViewModel.TryCloseAsync();
                }
            }

            await base.OnDeactivateAsync(
                close,
                ct);
        }

        public override async Task<bool> CanCloseAsync(CancellationToken ct)
            => DialogService.ShowYesNoDialog(
                UIStrings.Input_AreYouSure,
                UIStrings.Input_AreYouSureYouWantToClose)
                && await base.CanCloseAsync(ct);

        public async Task CommitOrder()
        {
            using (DisableUIService.Disable())
            {
                InputItems.ForEach(x =>
                {
                    x.Product.TotalAmountSold += x.Amount;

                    if (!ApplicationContext.HasCrashed)
                    {
                        x.Product.VirtualAmountSold += x.Amount;
                    }

                    x.Amount = 0;
                });

                UpdateItems();

                await SaveDataAsync();
            } 
        }

        public Task Start()
        {
            StonkMarketManager.Start();

            IsRunning = true;

            return EventAggregator.PublishOnCurrentThreadAsync(new StartedMessage());
        }

        public async Task Stop()
        {
            if (!DialogService.ShowYesNoDialog(
                UIStrings.Input_AreYouSure,
                UIStrings.Input_AreYouSureYouWantToStop))
            {
                return;
            }

            using (DisableUIService.Disable())
            {
                await StonkMarketManager.StopAsync();

                IsRunning = false;
            }

            await EventAggregator.PublishOnCurrentThreadAsync(new StoppedMessage());
        }

        public async Task Reset()
        {
            if (!DialogService.ShowYesNoDialog(
                UIStrings.Input_AreYouSure,
                UIStrings.Input_AreYouSureYouWantToReset))
            {
                return;
            }

            using (DisableUIService.Disable())
            {
                PriceCalculator.ResetEntirely(
                    ApplicationContext.Products);

                await EventAggregator.PublishOnCurrentThreadAsync(new PricesUpdatedMessage());

                InputItems.Clear();
                UpdateItems();

                await SaveDataAsync();
            }
        }

        public Task ToggleChartWindowState()
            => EventAggregator.PublishOnCurrentThreadAsync(new ToggleChartWindowStateMessage());

        public async Task HandleAsync(
            PricesUpdatedMessage _,
            CancellationToken ct)
        {
            if (!ApplicationContext.Config.AllowPriceUpdatesDuringOrder
                && InputItems.Any(x => x.Amount != 0))
            {
                return;
            }

            UpdateItems();
        }

        private void UpdateItems()
        {
            InputItems = InputItemsFactory.Create(
                ApplicationContext.Products,
                InputItems,
                UpdateTotalPrice);

            UpdateTotalPrice();
        }

        private async Task SaveDataAsync()
        {
            var saveResult = await DataPersistenceHelper.SaveDataAsync();
            if (!saveResult.IsSuccess)
            {
                DialogService.ShowError(UIStrings.Error_CannotSaveData);
            }
        }

        private void UpdateTotalPrice()
            => TotalPriceString = PriceFormatHelper.Format(
                InputItems.Sum(
                    x => x.Amount * x.PriceInCents));
    }
}
