using System.Reflection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Stip.BattleGames.Common.Services;
using Stip.BattleGames.Common.Helpers;

namespace Stip.BattleGames.Common
{
    public static class DIModule
    {
        public static void RegisterServices(IServiceCollection serviceCollection)
        {
            var assembly = Assembly.GetExecutingAssembly();

            serviceCollection
                .AddSingleton<IMessenger>(StrongReferenceMessenger.Default)
                .AddSingleton<WindowManager>()
                .AddSingleton<DisableUIService>();

            DIModuleHelper.RegisterAllDerivingFrom<IInjectable>(
                serviceCollection,
                assembly,
                false,
                false);
        }
    }
}
