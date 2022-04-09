using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Stip.Stipstonks.UnitTests
{
    public static class ObjectExtensions
    {
        public static T ShallowClone<T>(this T instance)
            => new MapperConfiguration(x => x.CreateMap<T, T>())
                .CreateMapper()
                .Map<T>(instance);

        public static List<T> ShallowCloneEnumerable<T>(this IEnumerable<T> instances)
        {
            var mapper = new MapperConfiguration(x => x.CreateMap<T, T>())
                .CreateMapper();

            return instances.Select(x => mapper.Map<T>(x)).ToList();
        }

        public static bool DeeplyEquals<T>(this T instance, T other)
            => JsonSerializer.SerializeToUtf8Bytes(instance)
                .SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(other));
    }
}
