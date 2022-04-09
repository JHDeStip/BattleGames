using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stip.Stipstonks.Extensions;
using System;
using System.Linq;

namespace Stip.Stipstonks.UnitTests.Extensions
{
    [TestClass]
    public class ContainerExtensionsTests
    {
        [TestMethod]
        public void ResolveComponent_ShouldCallDisposeOnItem()
        {
            using (var container = new WindsorContainer())
            {
                container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

                container.Register(Component.For<ISimpleComponent>()
                    .ImplementedBy<SimpleComponent>()
                    .LifestyleTransient());

                ISimpleComponent component;
                using (container.ResolveComponent<ISimpleComponent>(out component))
                {
                    Assert.IsFalse(component.IsDisposed);
                }

                Assert.IsTrue(component.IsDisposed);
            }
        }

        [TestMethod]
        public void ResolveComponent_ShouldCallDisposeOnNestedItem()
        {
            using (var container = new WindsorContainer())
            {
                container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

                container.Register(Component.For<ISimpleComponent>()
                    .ImplementedBy<SimpleComponent>()
                    .LifestyleTransient());

                container.Register(Component.For<IComplexComponent>()
                    .ImplementedBy<ComplexComponent>()
                    .LifestyleTransient());

                IComplexComponent component;
                using (container.ResolveComponent<IComplexComponent>(out component))
                {
                    Assert.IsFalse(component.NestedComponents.Any(x => x.IsDisposed));
                }

                Assert.IsTrue(component.NestedComponents.All(x => x.IsDisposed));
            }
        }

        public interface ISimpleComponent : IDisposable
        {
            public bool IsDisposed { get; }
        }

        public class SimpleComponent : ISimpleComponent
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
                => IsDisposed = true;
        }

        public interface IComplexComponent
        {
            ISimpleComponent[] NestedComponents { get; set; }
        }

        public class ComplexComponent : IComplexComponent
        {
            public ISimpleComponent[] NestedComponents { get; set; }
        }
    }
}
