using System.Reflection;
using Stip.Stipstonks.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls;
using Stip.BattleGames.Common.Windows;
using Stip.BattleGames.Common;
using Stip.BattleGames.Common.Helpers;

namespace Stip.Stipstonks;

public static class DIModule
{
    public static void RegisterServices(
        IServiceCollection serviceCollection,
        AppBase app)
    {
        var assembly = Assembly.GetExecutingAssembly();

        serviceCollection
            .AddSingleton(app)
            .AddSingleton<ApplicationContext>()
            .AddSingleton<StonkMarketManager>();

        DIModuleHelper.RegisterAllDerivingFrom<ViewModelBase>(
            serviceCollection,
            assembly,
            true,
            true);

        DIModuleHelper.RegisterAllDerivingFrom<IInjectable>(
            serviceCollection,
            assembly,
            false,
            false);

        DIModuleHelper.RegisterAllDerivingFrom<Window>(
            serviceCollection,
            assembly,
            false,
            false);
    }
}
