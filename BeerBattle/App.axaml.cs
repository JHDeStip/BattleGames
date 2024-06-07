using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Stip.BattleGames.Common;
using Stip.BattleGames.Common.Factories;
using Stip.BattleGames.Common.Helpers;
using Stip.BattleGames.Common.Services;
using Stip.BeerBattle.Helpers;
using Stip.BeerBattle.Windows;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.BeerBattle;

public partial class App : AppBase
{
    private ServiceProvider _serviceProvider;
    private ServiceScopeFactory _serviceScopeFactory;

    public override async void Initialize()
    {
        base.Initialize();

        _serviceProvider = ConfigureServiceProvider();
        _serviceScopeFactory = _serviceProvider.GetRequiredService<ServiceScopeFactory>();

        _serviceProvider.GetRequiredService<JsonHelper>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.Exit += (_, _) => _serviceProvider.Dispose();
        }

        if (!(await InitializeApplicationContextAsync()).IsSuccess)
        {
            (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown(-1);
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

    private ServiceProvider ConfigureServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        BattleGames.Common.DIModule.RegisterServices(serviceCollection);
        DIModule.RegisterServices(serviceCollection, this);

        var serviceProviderOptions = new ServiceProviderOptions();

#if DEBUG
        serviceProviderOptions.ValidateScopes = true;
        serviceProviderOptions.ValidateOnBuild = true;
#endif

        return serviceCollection.BuildServiceProvider(serviceProviderOptions);
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
