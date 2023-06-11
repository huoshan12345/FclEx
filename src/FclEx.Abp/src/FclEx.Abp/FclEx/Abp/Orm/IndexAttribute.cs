using System;
using System.Linq;
using FclEx.Extensions;

namespace FclEx.Abp.Orm;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class IndexAttribute : Attribute
{
    public IndexAttribute(bool isUnique, params string?[] propertyNames)
    {
        if (propertyNames.IsNullOrEmpty() || propertyNames.Contains(null))
            throw new ArgumentNullException(nameof(propertyNames));

        var arr = propertyNames.Distinct().ToArray();

        if (arr.Length == 0)
            throw new ArgumentException("The array is empty", nameof(propertyNames));

        PropertyNames = arr!;
        IsUnique = isUnique;
    }

    public bool IsUnique { get; }

    public string[] PropertyNames { get; }
}