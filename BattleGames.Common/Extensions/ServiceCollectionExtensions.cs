using Microsoft.Extensions.DependencyInjection;

namespace Stip.BattleGames.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSingletonWithBaseType<TBase, T>(
        this IServiceCollection serviceCollection)
        where T : class
        => AddWithBaseType<TBase, T>(serviceCollection, true);

    public static IServiceCollection AddTransientWithBaseType<TBase, T>(
        this IServiceCollection serviceCollection)
        where T : class
        => AddWithBaseType<TBase, T>(serviceCollection, false);

    private static IServiceCollection AddWithBaseType<TBase, T>(
        this IServiceCollection serviceCollection,
        bool addAsSingleton)
        where T : class
    {
        if (addAsSingleton)
        {
            serviceCollection.AddSingleton<T>();
        }
        else
        {
            serviceCollection.AddTransient<T>();
        }

        serviceCollection.AddTransient(typeof(TBase), x => x.GetRequiredService<T>());

        return serviceCollection;
    }
}