using Caliburn.Micro;
using System.Reflection;
using Stip.Stipstonks.Services;
using Stip.Stipstonks.Windows;
using Stip.Stipstonks.Helpers;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Windows;
using System;

namespace Stip.Stipstonks
{
    public static class DIModule
    {
        public static void RegisterServices(
            IServiceCollection serviceCollection,
            IApp app)
        {
            var assembly = Assembly.GetExecutingAssembly();

            serviceCollection
                .AddSingleton(app)
                .AddSingleton<IMessenger>(StrongReferenceMessenger.Default)
                .AddSingleton<IWindowManager, WindowManager>()
                .AddSingleton<DisableUIService>()
                .AddSingleton<ApplicationContext>()
                .AddSingleton<StonkMarketManager>();

            RegisterAllDerivingFrom<ViewModelBase>(
                serviceCollection,
                assembly,
                true,
                true);

            RegisterAllDerivingFrom<IInjectable>(
                serviceCollection,
                assembly,
                false,
                false);

            RegisterAllDerivingFrom<Window>(
                serviceCollection,
                assembly,
                false,
                false);
        }

        private static void RegisterAllDerivingFrom<T>(
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

        private static bool IsRegisterableByBaseType<T>(Type type, IServiceCollection serviceCollection)
            => type.IsAssignableTo(typeof(T))
            && !type.IsInterface
            && !type.IsAbstract
            && !serviceCollection.Any(x => x.ImplementationType == type);
    }
}
