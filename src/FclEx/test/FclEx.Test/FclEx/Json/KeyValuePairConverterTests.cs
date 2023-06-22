using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FclEx.Json.Converters;
using Newtonsoft.Json;

namespace FclEx.Json;

public class KeyValuePairConverterTests
{
    private class MyList<T> : List<T>
    {
    }

    private class MyListWithCtor<T> : List<T>
    {
        public MyListWithCtor(IEnumerable<T> collection) : base(collection)
        {
        }
    }


    private static readonly MethodInfo _method = typeof(KeyValuePairConverterTests).GetRequiredMethod(nameof(ReadTestGeneric));

    private static readonly Type _kvRawType = typeof(KeyValuePair<,>);

    public static IEnumerable<int> Source { get; } = Enumerable.Range(1, 10);

    private static string ToCharStr(int i) => i.CastTo<char>().ToString();

    public static IDictionary[] Datas { get; } =
    {
        Source.ToDictionary(m => m, m => -m),
        Source.ToDictionary(m => m, m => ToCharStr(m + 'a' - 1)),
        Source.ToDictionary(m => ToCharStr(m + 'A' - 1), m => m),
        Source.ToDictionary(m => ToCharStr(m + 'A' - 1), m => ToCharStr(m + 'a' - 1)),
        Source.ToDictionary(m => m.ToString(), m => Source),
        Source.ToDictionary(m => m.ToString(), m => Source.ToDictionary(s => s)),
        Source.ToDictionary(m => (m + 'A' - 1).CastTo<char>(), m => m),
        Source.ToDictionary(m => (m + 'A' - 1).CastTo<char>(), m => (m + 'a' - 1).CastTo<char>()),
    };

    public static Func<Type, Type>[] KvToColConvertors { get; } =
    {
        t => t.MakeArrayType(),
        t => typeof(IEnumerable<>).MakeGenericType(t),
        t => typeof(ICollection<>).MakeGenericType(t),
        t => typeof(IList<>).MakeGenericType(t),
        t => typeof(List<>).MakeGenericType(t),
        t => typeof(IReadOnlyCollection<>).MakeGenericType(t),
        t => typeof(ReadOnlyCollection<>).MakeGenericType(t),
        t => typeof(IReadOnlyList<>).MakeGenericType(t),
        t => typeof(MyList<>).MakeGenericType(t),
        t => typeof(MyListWithCtor<>).MakeGenericType(t),
    };


    public static IEnumerable<object[]> Cases { get; } = Datas
        .SelectMany(m => KvToColConvertors.Select(c => (m, c)))
        .Select(m => new object[] { m.m, m.c }).ToArray();

    private static void ReadTestGeneric<T, TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> raw)
        where T : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : notnull
    {
        var json = raw.ToJson();
        var pairs = JsonConvert.DeserializeObject<T>(json, new KeyValuePairsConverter())!;

        var dic = raw.ToDictionary(m => m.Key, m => m.Value);

        var i = 0;
        foreach (var pair in pairs)
        {
            Assert.True(dic.TryGetValue(pair.Key, out var value));
            Assert.Equal(value, pair.Value);
            ++i;
        }
        Assert.Equal(dic.Count, i);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void ReadTest(IDictionary dic, Func<Type, Type> toColType)
    {
        var dicType = dic.GetType();
        var keyType = dicType.GenericTypeArguments[0];
        var valueType = dicType.GenericTypeArguments[1];
        var kvType = _kvRawType.MakeGenericType(keyType, valueType);
        var colType = toColType(kvType);
        _method.MakeGenericMethod(colType, keyType, valueType)
            .Invoke(null, new object[] { dic });
    }
}