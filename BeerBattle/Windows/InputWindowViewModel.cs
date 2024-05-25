using CommunityToolkit.Mvvm.Messaging;
using Stip.BattleGames.Common;
using Stip.BattleGames.Common.Factories;
using Stip.BattleGames.Common.Messages;
using Stip.BattleGames.Common.Services;
using Stip.BattleGames.Common.Windows;
using Stip.BeerBattle.Factories;
using Stip.BeerBattle.Helpers;
using Stip.BeerBattle.Items;
using Stip.BeerBattle.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.BeerBattle.Windows;

public class InputWindowViewModel(
    ApplicationContext _applicationContext,
    IMessenger _messenger,
    DataPersistenceHelper _dataPersistenceHelper,
    PointsCalculator _pointsCalculator,
    PointsFormatHelper _pointsFormatHelper,
    DisableUIService _disableUIService,
    DialogService _dialogService,
    ServiceScopeFactory _serviceScopeFactory,
    InputItemsFactory _inputItemsFactory)
    : ViewModelBase,
    IUIEnabled
{

    private string _backgroundColor;
    public string BackgroundColor { get => _backgroundColor; set => SetProperty(ref _backgroundColor, value); }

    private bool _uiEnabled = true;
    public bool UIEnabled { get => _uiEnabled; set => SetProperty(ref _uiEnabled, value); }

    private IReadOnlyList<GroupItem> _groupItems = [];
    public IReadOnlyList<GroupItem> GroupItems { get => _groupItems; set => SetProperty(ref _groupItems, value); }

    private IReadOnlyList<InputItem> _inputItems = [];
    public IReadOnlyList<InputItem> InputItems { get => _inputItems; set => SetProperty(ref _inputItems, value); }

    private string _totalPointsString;
    public string TotalPointsString { get => _totalPointsString; private set => SetProperty(ref _totalPointsString, value); }

    public override async ValueTask InitializeAsync(CancellationToken ct)
    {
        await base.InitializeAsync(ct);

        BackgroundColor = _applicationContext.Config.WindowBackgroundColor;
    }

    public override async ValueTask ActivateAsync(CancellationToken ct)
    {
        await base.ActivateAsync(ct);

        _messenger.RegisterAll(this);
        InitializeGroupItems();
        InitializeInputItems();
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

    public async Task CommitOrder(GroupItem groupItem)
    {
        using (_disableUIService.Disable())
        {
            foreach (var inputItem in InputItems)
            {
                groupItem.Group.TotalPoints += inputItem.Amount * inputItem.Product.PointsPerItem;
                inputItem.Amount = 0;
            }

            _pointsCalculator.CalculatePointLevels(
                _applicationContext.Groups);

            _messenger.Send<PointsUpdatedMessage>();

            await SaveDataAsync();
        } 
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
            _pointsCalculator.Reset(
                _applicationContext.Groups);

            _messenger.Send<PointsUpdatedMessage>();

            await SaveDataAsync();
        }
    }

    public void ToggleChartWindowState()
        => _messenger.Send<ToggleChartWindowStateMessage>();

    private void InitializeGroupItems()
        => GroupItems = _applicationContext.Groups.Select(GroupItem.From).ToList();

    private void InitializeInputItems()
    {
        InputItems = _inputItemsFactory.Create(
            _applicationContext.Products,
            UpdateTotalPoints);

        UpdateTotalPoints();
    }

    private async Task SaveDataAsync()
    {
        var saveResult = await _dataPersistenceHelper.SaveDataAsync();
        if (!saveResult.IsSuccess)
        {
            await _dialogService.ShowErrorAsync(BattleGames.Common.UIStrings.Error_CannotSaveData);
        }
    }

    private void UpdateTotalPoints()
        => TotalPointsString = _pointsFormatHelper.Format(
            InputItems.Sum(
                x => x.Amount * x.Product.PointsPerItem));
}
