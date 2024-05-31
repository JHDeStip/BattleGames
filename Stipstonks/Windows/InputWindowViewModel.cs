using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Stip.BattleGames.Common;
using Stip.BattleGames.Common.Factories;
using Stip.BattleGames.Common.Messages;
using Stip.BattleGames.Common.Services;
using Stip.BattleGames.Common.Windows;
using Stip.Stipstonks.Factories;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Items;
using Stip.Stipstonks.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Windows;

public partial class InputWindowViewModel(
    ApplicationContext _applicationContext,
    IMessenger _messenger,
    DataPersistenceHelper _dataPersistenceHelper,
    StonkMarketManager _stonkMarketManager,
    PriceCalculator _priceCalculator,
    PriceFormatHelper _priceFormatHelper,
    DisableUIService _disableUIService,
    DialogService _dialogService,
    ServiceScopeFactory _serviceScopeFactory,
    InputItemsFactory _inputItemsFactory)
    : ViewModelBase,
    IUIEnabled,
    IRecipient<PricesUpdatedMessage>
{
    [ObservableProperty]
    private string _backgroundColor;

    [ObservableProperty]
    private bool _UIEnabled = true;

    [ObservableProperty]
    private IReadOnlyList<InputItem> _inputItems = [];

    [ObservableProperty]
    private string _totalPriceString;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotRunning))]
    private bool _isRunning;

    public bool IsNotRunning => !IsRunning;

    public override async ValueTask InitializeAsync(CancellationToken ct)
    {
        await base.InitializeAsync(ct);

        BackgroundColor = _applicationContext.Config.WindowBackgroundColor;
    }

    public override async ValueTask ActivateAsync(CancellationToken ct)
    {
        await base.ActivateAsync(ct);

        _messenger.RegisterAll(this);
        UpdateInputItems();
    }

    public override async ValueTask DeactivateAsync(CancellationToken ct)
    {
        _messenger.UnregisterAll(this);

        await using (var scope = _serviceScopeFactory.CreateAsyncScope())
        {
            await scope
                .GetRequiredService<ChartWindowViewModel>()
                .CloseAsync(ct);
        }

        await base.DeactivateAsync(ct);
    }

    public override async ValueTask<bool> CanDeactivateAsync(CancellationToken ct)
        => await _dialogService.ShowYesNoDialogAsync(
            BattleGames.Common.UIStrings.Input_AreYouSure,
            BattleGames.Common.UIStrings.Input_AreYouSureYouWantToClose)
        && await base.CanDeactivateAsync(ct);

    public async Task CommitOrder()
    {
        using (_disableUIService.Disable())
        {
            foreach (var inputItem in InputItems)
            {
                inputItem.Product.TotalAmountSold += inputItem.Amount;

                if (!_applicationContext.HasCrashed)
                {
                    inputItem.Product.VirtualAmountSold += inputItem.Amount;
                }

                inputItem.Amount = 0;
            }

            UpdateInputItems();

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
        if (!await _dialogService.ShowYesNoDialogAsync(
            BattleGames.Common.UIStrings.Input_AreYouSure,
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
        if (!await _dialogService.ShowYesNoDialogAsync(
            BattleGames.Common.UIStrings.Input_AreYouSure,
            BattleGames.Common.UIStrings.Input_AreYouSureYouWantToReset))
        {
            return;
        }

        using (_disableUIService.Disable())
        {
            _priceCalculator.ResetEntirely(
                _applicationContext.Products);

            _messenger.Send<PricesUpdatedMessage>();

            UpdateInputItems();

            await SaveDataAsync();
        }
    }

    public void ToggleChartWindowState()
        => _messenger.Send<ToggleChartWindowStateMessage>();

    public void Receive(PricesUpdatedMessage message)
        => Dispatcher.UIThread.Invoke(() =>
        {
            if (!_applicationContext.Config.AllowPriceUpdatesDuringOrder
                && InputItems.Any(x => x.Amount != 0))
            {
                return;
            }

            UpdateInputItems();
        });

    private void UpdateInputItems()
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
            await _dialogService.ShowErrorAsync(BattleGames.Common.UIStrings.Error_CannotSaveData);
        }
    }

    private void UpdateTotalPrice()
        => TotalPriceString = _priceFormatHelper.Format(
            InputItems.Sum(
                x => x.Amount * x.PriceInCents));
}
