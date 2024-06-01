using Microsoft.Extensions.DependencyInjection;
using Stip.BattleGames.Common.Windows;
using Stip.BattleGames.Common;
using Stip.BeerBattle.Helpers;
using Stip.BeerBattle.Factories;
using Stip.BeerBattle.Windows;
using Stip.BattleGames.Common.Extensions;

namespace Stip.BeerBattle;

public static class DIModule
{
    public static void RegisterServices(
        IServiceCollection serviceCollection,
        AppBase app)
        => serviceCollection
        .AddSingleton(app)
        .AddSingleton<ApplicationContext>()
        .AddSingleton<PointsFormatHelper>()
        .AddTransient<InputItemsFactory>()
        .AddTransient<DataPersistenceHelper>()
        .AddTransient<PointsCalculator>()
        .AddTransient<ChartWindowView>()
        .AddTransient<InputWindowView>()
        .AddSingletonWithBaseType<ViewModelBase, ChartWindowViewModel>()
        .AddSingletonWithBaseType<ViewModelBase, InputWindowViewModel>();
}
