﻿using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Stip.BattleGames.Common;
using Stip.BattleGames.Common.Factories;
using Stip.BattleGames.Common.Services;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Windows;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks;

public class Bootstrapper
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ServiceScopeFactory _serviceScopeFactory;

    public Bootstrapper()
    {
        SetCulture();

        _serviceProvider = ConfigurServiceProvider();
        _serviceScopeFactory = _serviceProvider.GetRequiredService<ServiceScopeFactory>();

        ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Exit +=
            (_, _) => _serviceProvider.Dispose();

        Start();
    }

    private static void SetCulture()
    {
        var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        culture.NumberFormat.CurrencySymbol = UIStrings.Global_CurrencySymbol;

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.CurrentCulture = culture;

        var uiCulture = (CultureInfo)CultureInfo.CurrentUICulture.Clone();
        uiCulture.NumberFormat.CurrencySymbol = UIStrings.Global_CurrencySymbol;

        CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
        CultureInfo.CurrentUICulture = uiCulture;
    }

    private static ServiceProvider ConfigurServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        BattleGames.Common.DIModule.RegisterServices(serviceCollection);
        DIModule.RegisterServices(serviceCollection, (AppBase)Application.Current);

        var serviceProviderOptions = new ServiceProviderOptions();
#if DEBUG
        serviceProviderOptions.ValidateScopes = true;
        serviceProviderOptions.ValidateOnBuild = true;
#endif
        return serviceCollection.BuildServiceProvider(serviceProviderOptions);
    }

    private async Task Start()
    {
        if (!(await InitializeApplicationContextAsync()).IsSuccess)
        {
            ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Shutdown(-1);
            return;
        }

        await InitializeDisableUIServiceAsync();

        await using (var scope = _serviceScopeFactory.CreateAsyncScope())
        {
            var windowManager = scope.GetRequiredService<WindowManager>();

            var inputWindowTask = await windowManager.ShowWindowAsync(
                scope.GetRequiredService<InputWindowView>(),
                CancellationToken.None);

            var chartWindowTask = await windowManager.ShowWindowAsync(
                scope.GetRequiredService<ChartWindowView>(),
                CancellationToken.None);

            await Task.WhenAll(inputWindowTask, chartWindowTask);
        }
    }

    private async Task<ActionResult> InitializeApplicationContextAsync()
    {
        await using (var scope = _serviceScopeFactory.CreateAsyncScope())
        {
            var loadResult = await scope
                .GetRequiredService<DataPersistenceHelper>()
                .LoadDataAsync();
            if (!loadResult.IsSuccess)
            {
                await scope
                    .GetRequiredService<DialogService>()
                    .ShowErrorAsync(BattleGames.Common.UIStrings.Error_CannotLoadData);
                return ActionResult.Failure;
            }

            var applicationContext = scope.GetRequiredService<ApplicationContext>();
            scope
                .GetRequiredService<PriceCalculator>()
                .RecalculatePrices(
                    applicationContext.Products,
                    applicationContext.Config.MaxPriceDeviationFactor,
                    applicationContext.Config.PriceResolutionInCents);
        }

        return ActionResult.Success;
    }

    private async Task InitializeDisableUIServiceAsync()
    {
        await using (var scope = _serviceScopeFactory.CreateAsyncScope())
        {
            scope
                .GetRequiredService<DisableUIService>()
                .PushViewModel(
                    scope.GetRequiredService<InputWindowViewModel>());
        }
    }
}
