using System.Collections.Generic;
using System;
using System.Linq;
using Volo.Abp.Reflection;

namespace FclEx.Abp;

public static class TypeExtensions
{
    public static IReadOnlyList<Type> GetEntityTypes(this ITypeFinder typeFinder)
    {
        return typeFinder.Types.Where(m => m.IsEntity()).ToList();
    }
}