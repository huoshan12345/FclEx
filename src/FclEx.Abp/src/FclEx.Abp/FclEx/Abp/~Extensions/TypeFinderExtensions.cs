using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Reflection;

namespace FclEx.Abp;

public static class TypeFinderExtensions
{
    public static IReadOnlyList<Type> GetEntityTypes(this ITypeFinder typeFinder)
    {
        return typeFinder.Types.Where(m => m.IsEntity()).ToList();
    }
}