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
    public class InputWindowViewModel
        : ViewModelBase,
        IUIEnabled,
        IRecipient<PricesUpdatedMessage>
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

        private IReadOnlyList<InputItem> _inputItems = new List<InputItem>();
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

            Messenger.RegisterAll(this);
            UpdateItems();
        }

        protected override async Task OnDeactivateAsync(
            bool close,
            CancellationToken ct)
        {
            Messenger.UnregisterAll(this);

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
                InputItems.Apply(x =>
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

        public void Start()
        {
            StonkMarketManager.Start();

            IsRunning = true;

            Messenger.Send<StartedMessage>();
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

            Messenger.Send<StoppedMessage>();
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

                Messenger.Send<PricesUpdatedMessage>();

                UpdateItems();

                await SaveDataAsync();
            }
        }

        public void ToggleChartWindowState()
            => Messenger.Send<ToggleChartWindowStateMessage>();

        public void Receive(PricesUpdatedMessage message)
            => OnUIThread(() =>
            {
                if (!ApplicationContext.Config.AllowPriceUpdatesDuringOrder
                                && InputItems.Any(x => x.Amount != 0))
                {
                    return;
                }

                UpdateItems();
            });

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
