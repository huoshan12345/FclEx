using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FclEx;
using FclEx.Abp.Domain.Entities.Interfaces;

namespace Volo.Abp.Reflection
{
    public static class Extensions
    {
        internal static Type EntityType { get; } = typeof(IEntity);

        public static IReadOnlyList<Type> GetEntityTypes(this ITypeFinder typeFinder)
        {
            return typeFinder.Types.Where(m => m.IsEntity()).ToList();
        }

        public static bool IsEntity(this Type type)
        {
            return !type.IsGenericType && !type.IsAbstract && EntityType.IsAssignableFrom(type);
        }
    }
}
