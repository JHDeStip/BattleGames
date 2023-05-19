using Castle.Windsor;
using System;
using System.Collections;

namespace Stip.Stipstonks.Extensions
{
    public class ContainerComponent<T> : IDisposable
    {
        public T Component { get; }

        private readonly IWindsorContainer _container;

        public ContainerComponent(IWindsorContainer container, T component)
        {
            _container = container;
            Component = component;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (Component is not IEnumerable collection)
            {
                _container.Release(Component);
                return;
            }

            ReleaseCollection(collection);
        }

        private void ReleaseCollection(IEnumerable collection)
        {
            foreach (var item in collection)
            {
                if (item is IEnumerable nestedCollection)
                {
                    ReleaseCollection(nestedCollection);
                }

                _container.Release(item);
            }
        }
    }
}
