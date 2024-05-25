using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Stip.BattleGames.Common;
using Stip.BattleGames.Common.Factories;
using Stip.BattleGames.Common.Services;
using Stip.BeerBattle.Helpers;
using Stip.BeerBattle.Windows;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.BeerBattle;

public class Bootstrapper
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ServiceScopeFactory _serviceScopeFactory;

    public Bootstrapper()
    {
        _serviceProvider = ConfigurServiceProvider();
        _serviceScopeFactory = _serviceProvider.GetRequiredService<ServiceScopeFactory>();

        ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Exit +=
            (_, _) => _serviceProvider.Dispose();

        Start();
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

            scope.GetRequiredService<PointsFormatHelper>().Initialize(
                applicationContext.Config);

            scope.GetRequiredService<PointsCalculator>().CalculatePointLevels(
                applicationContext.Groups);
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
