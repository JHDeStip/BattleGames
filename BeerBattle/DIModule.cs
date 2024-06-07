using Microsoft.Extensions.DependencyInjection;
using Stip.BattleGames.Common;
using Stip.BeerBattle.Helpers;
using Stip.BeerBattle.Factories;
using Stip.BeerBattle.Windows;

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
        .AddSingleton<ChartWindowViewModel>()
        .AddSingleton<InputWindowViewModel>();
}
