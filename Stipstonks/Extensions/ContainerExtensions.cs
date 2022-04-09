using Castle.Windsor;

namespace Stip.Stipstonks.Extensions
{
    public static class ContainerExtensions
    {
        public static ContainerComponent<T> ResolveComponent<T>(
            this IWindsorContainer container,
            out T component)
        {
            component = container.Resolve<T>();

            return new ContainerComponent<T>(container, component);
        }
    }
}
