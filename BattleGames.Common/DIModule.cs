using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Stip.BattleGames.Common.Services;
using Stip.BattleGames.Common.Helpers;
using Stip.BattleGames.Common.Factories;

namespace Stip.BattleGames.Common;

public static class DIModule
{
    public static void RegisterServices(IServiceCollection serviceCollection)
        => serviceCollection
        .AddSingleton<IMessenger>(StrongReferenceMessenger.Default)
        .AddSingleton<WindowManager>()
        .AddSingleton<DisableUIService>()
        .AddTransient<ServiceScopeFactory>()
        .AddTransient<EnvironmentHelper>()
        .AddTransient<FileHelper>()
        .AddTransient<JsonHelper>()
        .AddTransient<DialogService>();
}
