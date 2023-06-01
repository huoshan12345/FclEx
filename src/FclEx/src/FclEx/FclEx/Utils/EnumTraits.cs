using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FclEx.Helpers;

namespace FclEx.Utils;

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
public static class EnumTraits<TEnum> where TEnum : struct, Enum
{
    private static readonly HashSet<TEnum> _valuesSet;
    static EnumTraits()
    {
        EnumValues = Enum.GetValues<TEnum>();
        _valuesSet = new HashSet<TEnum>(EnumValues);

        var longValues = EnumValues
            .Select(v => v.ToLong())
            .ToList();

        IsEmpty = longValues.Count == 0;
        if (!IsEmpty)
        {
            var sorted = longValues.OrderBy(v => v).ToList();
            MinValue = sorted.Min();
            MaxValue = sorted.Max();
        }
    }

    public static bool IsEmpty { get; }
    public static long MinValue { get; }
    public static long MaxValue { get; }
    public static TEnum[] EnumValues { get; }

    // This version is almost an order of magnitude faster then Enum.IsDefined
    public static bool IsValid(TEnum value) => _valuesSet.Contains(value);
}