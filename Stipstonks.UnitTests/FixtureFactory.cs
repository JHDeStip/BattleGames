using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Kernel;

namespace Stip.Stipstonks.UnitTests
{
    public static class FixtureFactory
    {
        private static readonly Random Random = new();

        public static Fixture Create()
        {
            var fixture = new Fixture();

            fixture.Customize<double>(x => x.FromFactory<int>(y => y * Random.NextDouble()));

            fixture.Customizations.Add(new IReadOnlyListResolver());
            fixture.Customizations.Add(new DependencyObjectOmiter());
            fixture.Customizations.Add(new StyleOmitter());
            fixture.Customizations.Add(new UnmockedDependencyOmitter(fixture));

            return fixture;
        }

        private class IReadOnlyListResolver : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (request is PropertyInfo propertyInfo
                    && propertyInfo.PropertyType.IsGenericType
                    && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
                {
                    return context.Resolve(typeof(List<>).MakeGenericType(propertyInfo.PropertyType.GetGenericArguments()));
                }

                return new NoSpecimen();
            }
        }

        private class DependencyObjectOmiter : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext _)
                => OmitPropertyType(request, x => typeof(DependencyObject).IsAssignableFrom(x));
        }

        private class StyleOmitter : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext _)
                => OmitPropertyType(request, x => typeof(Style).IsAssignableFrom(x));
        }

        private class UnmockedDependencyOmitter : ISpecimenBuilder
        {
            private readonly Fixture _fixture;

            public UnmockedDependencyOmitter(Fixture fixture)
                => _fixture = fixture;

            public object Create(object request, ISpecimenContext _)
                => OmitPropertyType(request, x
                    => (x.IsInterface
                        || typeof(IInjectable).IsAssignableFrom(x))
                        && !_fixture.Customizations.Any(y =>
                        {
                            var customizationType = y.GetType();
                            return customizationType.IsGenericType
                                && customizationType.GetGenericTypeDefinition() == typeof(NodeComposer<>)
                                && customizationType.GetGenericArguments().FirstOrDefault() == x;
                        }));
        }

        private static object OmitPropertyType(object request, Func<Type, bool> predicate)
            => request is PropertyInfo propertyInfo
            && predicate(propertyInfo.PropertyType)
                ? (object)new OmitSpecimen()
                : new NoSpecimen();
    }
}
