using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Kernel;
using Avalonia;
using Stip.BattleGames.Common;

namespace Stip.BattleGames.UnitTestsCommon;

public static class FixtureFactory
{
    private static readonly Random Random = new();

    public static Fixture Create()
    {
        var fixture = new Fixture();

        fixture.Customize<double>(x => x.FromFactory<int>(y => y * Random.NextDouble()));

        fixture.Customizations.Add(new IReadOnlyListResolver());
        fixture.Customizations.Add(new AvaloniaObjectOmiter());
        fixture.Customizations.Add(new UnmockedConstructorDependencyOmitter(fixture));

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
                var items = context.Resolve(typeof(IEnumerable<>).MakeGenericType(propertyInfo.PropertyType.GetGenericArguments()));
                return typeof(List<>).MakeGenericType(propertyInfo.PropertyType.GetGenericArguments()).GetConstructor([items.GetType()]).Invoke([items]);
            }

            return new NoSpecimen();
        }
    }

    private class AvaloniaObjectOmiter : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext _)
            => OmitPropertyType(request, x => typeof(AvaloniaObject).IsAssignableFrom(x));
    }

    private class UnmockedConstructorDependencyOmitter(
        Fixture _fixture)
        : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext _)
            => OmitParameterType(request, x => IsDependency(x, _fixture));
    }

    private static NoSpecimen OmitPropertyType(object request, Func<Type, bool> predicate)
        => request is PropertyInfo propertyInfo
        && predicate(propertyInfo.PropertyType)
            ? null
            : new NoSpecimen();

    private static NoSpecimen OmitParameterType(object request, Func<Type, bool> predicate)
        => request is ParameterInfo parameterInfo
        && predicate(parameterInfo.ParameterType)
            ? null
            : new NoSpecimen();

    private static bool IsDependency(Type type, Fixture fixture)
        => (type.IsInterface
        || typeof(IInjectable).IsAssignableFrom(type))
        && !fixture.Customizations.Any(y =>
        {
            var customizationType = y.GetType();
            return customizationType.IsGenericType
                && customizationType.GetGenericTypeDefinition() == typeof(NodeComposer<>)
                && customizationType.GetGenericArguments().FirstOrDefault() == type;
        });
}
