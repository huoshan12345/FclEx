using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FclEx.Xunit;

internal static class FclExXunitHelper
{
    public static IEnumerable<FieldInfo> GetAllFields(Type type)
    {
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        if (type.BaseType is {} baseType)
        {
            return fields.Concat(GetAllFields(baseType));
        }

        return fields;
    }
}
