using Caliburn.Micro;
using Castle.Windsor;
using CommunityToolkit.Mvvm.Messaging;
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
    public class InputWindowViewModel(
        ApplicationContext _applicationContext,
        IMessenger _messenger,
        DataPersistenceHelper _dataPersistenceHelper,
        StonkMarketManager _stonkMarketManager,
        PriceCalculator _priceCalculator,
        PriceFormatHelper _priceFormatHelper,
        DisableUIService _disableUIService,
        DialogService _dialogService,
        IWindsorContainer _container,
        InputItemsFactory _inputItemsFactory)
        : ViewModelBase,
        IUIEnabled,
        IRecipient<PricesUpdatedMessage>
    {
        public string BackgroundColor { get; private set; }

        private bool _uiEnabled = true;
        public bool UIEnabled { get => _uiEnabled; set => Set(ref _uiEnabled, value); }

        private IReadOnlyList<InputItem> _inputItems = [];
        public IReadOnlyList<InputItem> InputItems { get => _inputItems; set => Set(ref _inputItems, value); }

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

        protected override async Task OnInitializeAsync(CancellationToken ct)
        {
            await base.OnInitializeAsync(ct);

            BackgroundColor = _applicationContext.Config.WindowBackgroundColor;
        }

        protected override async Task OnActivateAsync(CancellationToken ct)
        {
            await base.OnActivateAsync(ct);

            _messenger.RegisterAll(this);
            UpdateItems();
        }

        protected override async Task OnDeactivateAsync(
            bool close,
            CancellationToken ct)
        {
            _messenger.UnregisterAll(this);

            if (close)
            {
                using (_container.ResolveComponent<ChartWindowViewModel>(out var chartWindowViewModel))
                {
                    await chartWindowViewModel.TryCloseAsync();
                }
            }

            await base.OnDeactivateAsync(
                close,
                ct);
        }

        public override async Task<bool> CanCloseAsync(CancellationToken ct)
            => _dialogService.ShowYesNoDialog(
                UIStrings.Input_AreYouSure,
                UIStrings.Input_AreYouSureYouWantToClose)
                && await base.CanCloseAsync(ct);

        public async Task CommitOrder()
        {
            using (_disableUIService.Disable())
            {
                InputItems.Apply(x =>
                {
                    x.Product.TotalAmountSold += x.Amount;

                    if (!_applicationContext.HasCrashed)
                    {
                        x.Product.VirtualAmountSold += x.Amount;
                    }

                    x.Amount = 0;
                });

                UpdateItems();

                await SaveDataAsync();
            } 
        }

        public void Start()
        {
            _stonkMarketManager.Start();

            IsRunning = true;

            _messenger.Send<StartedMessage>();
        }

        public async Task Stop()
        {
            if (!_dialogService.ShowYesNoDialog(
                UIStrings.Input_AreYouSure,
                UIStrings.Input_AreYouSureYouWantToStop))
            {
                return;
            }

            using (_disableUIService.Disable())
            {
                await _stonkMarketManager.StopAsync();

                IsRunning = false;
            }

            _messenger.Send<StoppedMessage>();
        }

        public async Task Reset()
        {
            if (!_dialogService.ShowYesNoDialog(
                UIStrings.Input_AreYouSure,
                UIStrings.Input_AreYouSureYouWantToReset))
            {
                return;
            }

            using (_disableUIService.Disable())
            {
                _priceCalculator.ResetEntirely(
                    _applicationContext.Products);

                _messenger.Send<PricesUpdatedMessage>();

                UpdateItems();

                await SaveDataAsync();
            }
        }

        public void ToggleChartWindowState()
            => _messenger.Send<ToggleChartWindowStateMessage>();

        public void Receive(PricesUpdatedMessage message)
            => OnUIThread(() =>
            {
                if (!_applicationContext.Config.AllowPriceUpdatesDuringOrder
                    && InputItems.Any(x => x.Amount != 0))
                {
                    return;
                }

                UpdateItems();
            });

        private void UpdateItems()
        {
            InputItems = _inputItemsFactory.Create(
                _applicationContext.Products,
                InputItems,
                UpdateTotalPrice);

            UpdateTotalPrice();
        }

        private async Task SaveDataAsync()
        {
            var saveResult = await _dataPersistenceHelper.SaveDataAsync();
            if (!saveResult.IsSuccess)
            {
                _dialogService.ShowError(UIStrings.Error_CannotSaveData);
            }
        }

        private void UpdateTotalPrice()
            => TotalPriceString = _priceFormatHelper.Format(
                InputItems.Sum(
                    x => x.Amount * x.PriceInCents));
    }
}
