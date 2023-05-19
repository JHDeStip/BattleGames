using Caliburn.Micro;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System.Reflection;
using System;
using Stip.Stipstonks.Services;
using Stip.Stipstonks.Windows;
using Stip.Stipstonks.Helpers;

namespace Stip.Stipstonks
{
    public class DIModule : IWindsorInstaller
    {
        private readonly IApp _app;

        public DIModule(IApp app)
            => _app = app;

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var assembly = Assembly.GetExecutingAssembly();

            InstallInfrastructure(container);
            container.Register(Component.For<ApplicationContext>().LifestyleSingleton());
            container.Register(Component.For<StonkMarketManager>().LifestyleSingleton());

            InstallAllByBaseTypeSingleton<ViewModelBase>(container, assembly);
            InstallAllByBaseType<IInjectable>(container, assembly);
        }

        private void InstallInfrastructure(IWindsorContainer container)
        {
            container.Register(Component.For<IApp>().Instance(_app));
            container.Register(Component.For<IWindsorContainer>().Instance(container));
            container.Register(Component.For<IEventAggregator>().ImplementedBy<EventAggregator>().LifestyleSingleton());
            container.Register(Component.For<IWindowManager>().ImplementedBy<WindowManager>().LifestyleSingleton());
            container.Register(Component.For<DisableUIService>().LifestyleSingleton());
        }

        protected static void InstallAllByConvention(IWindsorContainer container, Assembly assembly, string nameEnd)
            => container.Register(GetAllByConvention(assembly, nameEnd).LifestyleTransient());

        private static void InstallAllByBaseType<T>(IWindsorContainer container, Assembly assembly)
            => InstallAllByBaseType(container, assembly, typeof(T));

        private static void InstallAllByBaseType(IWindsorContainer container, Assembly assembly, Type baseType)
            => container.Register(GetAllByBaseType(assembly, baseType).LifestyleTransient());

        private static void InstallAllByBaseTypeSingleton<T>(IWindsorContainer container, Assembly assembly)
            => InstallAllByBaseTypeSingleton(container, assembly, typeof(T));

        private static void InstallAllByBaseTypeSingleton(IWindsorContainer container, Assembly assembly, Type baseType)
            => container.Register(GetAllByBaseType(assembly, baseType).LifestyleSingleton());

        private static BasedOnDescriptor GetAllByConvention(Assembly assembly, string nameEnd)
            => Classes.FromAssembly(assembly)
                .Where(x => x.Name.EndsWith(nameEnd));

        private static BasedOnDescriptor GetAllByBaseType(Assembly assembly, Type baseType)
            => Classes.FromAssembly(assembly)
                .BasedOn(baseType);
    }
}
