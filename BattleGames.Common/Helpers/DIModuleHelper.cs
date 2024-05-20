using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Stip.BattleGames.Common.Helpers;

public static class DIModuleHelper
{
    public static void RegisterAllDerivingFrom<T>(
        IServiceCollection serviceCollection,
        Assembly assembly,
        bool registerAsSingleton,
        bool registerByBaseType)
    {
        var typesToAdd = assembly.GetTypes().Where(x => IsRegisterableByBaseType<T>(x, serviceCollection));

        foreach (var typeToAdd in typesToAdd)
        {
            if (registerAsSingleton)
            {
                serviceCollection.AddSingleton(typeToAdd, typeToAdd);
            }
            else
            {
                serviceCollection.AddTransient(typeToAdd, typeToAdd);
            }

            if (registerByBaseType)
            {
                serviceCollection.AddTransient(typeof(T), x => x.GetRequiredService(typeToAdd));
            }
        }
    }

    public static bool IsRegisterableByBaseType<T>(Type type, IServiceCollection serviceCollection)
        => type.IsAssignableTo(typeof(T))
        && !type.IsInterface
        && !type.IsAbstract
        && !serviceCollection.Any(x => x.ImplementationType == type);
}
