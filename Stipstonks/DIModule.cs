using Stip.Stipstonks.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Stip.BattleGames.Common.Windows;
using Stip.BattleGames.Common;
using Stip.Stipstonks.Windows;
using Stip.Stipstonks.Factories;
using Stip.BattleGames.Common.Extensions;

namespace Stip.Stipstonks;

public static class DIModule
{
    public static void RegisterServices(
        IServiceCollection serviceCollection,
        AppBase app)
        => serviceCollection
        .AddSingleton(app)
        .AddSingleton<ApplicationContext>()
        .AddSingleton<StonkMarketManager>()
        .AddTransient<InputItemsFactory>()
        .AddTransient<PeriodicTimerFactory>()
        .AddTransient<CrashManager>()
        .AddTransient<DataPersistenceHelper>()
        .AddTransient<DelayHelper>()
        .AddTransient<MathHelper>()
        .AddTransient<PriceCalculator>()
        .AddTransient<PriceCalculatorHelper>()
        .AddTransient<PriceFormatHelper>()
        .AddTransient<PriceUpdateManager>()
        .AddTransient<ChartWindowView>()
        .AddTransient<InputWindowView>()
        .AddSingletonWithBaseType<ViewModelBase, ChartWindowViewModel>()
        .AddSingletonWithBaseType<ViewModelBase, InputWindowViewModel>();
}
