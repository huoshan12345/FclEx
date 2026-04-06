using FclEx.Xunit;

namespace FclEx.NewtonsoftJson;

public partial class KeyValuePairConverterTests
{
    private class MyList<T> : List<T>;

    private class MyListWithCtor<T>(IEnumerable<T> collection) : List<T>(collection);

    private static readonly MethodInfo _method = typeof(KeyValuePairConverterTests).GetRequiredMethod(nameof(ReadTestGeneric));

    private static readonly Type _kvRawType = typeof(KeyValuePair<,>);

    public static IEnumerable<int> Source { get; } = Enumerable.Range(1, 10);

    private static string ToCharStr(int i) => i.CastTo<char>().ToString();

    public static readonly IDictionary[] Dictionaries =
    [
        Source.ToDictionary(m => m, m => -m),
        Source.ToDictionary(m => m, m => ToCharStr(m + 'a' - 1)),
        Source.ToDictionary(m => ToCharStr(m + 'A' - 1), m => m),
        Source.ToDictionary(m => ToCharStr(m + 'A' - 1), m => ToCharStr(m + 'a' - 1)),
        Source.ToDictionary(m => m.ToString(), m => Source),
        Source.ToDictionary(m => m.ToString(), m => Source.ToDictionary(s => s)),
        Source.ToDictionary(m => (m + 'A' - 1).CastTo<char>(), m => m),
        Source.ToDictionary(m => (m + 'A' - 1).CastTo<char>(), m => (m + 'a' - 1).CastTo<char>()),
    ];

    public static readonly (string Name, Func<Type, Type> Converter)[] KvToColConverters =
    [
        (nameof(Array), t => t.MakeArrayType()),
        (nameof(IEnumerable<int>), t => typeof(IEnumerable<>).MakeGenericType(t)),
        (nameof(ICollection<int>), t => typeof(ICollection<>).MakeGenericType(t)),
        (nameof(IList<int>), t => typeof(IList<>).MakeGenericType(t)),
        (nameof(List<int>), t => typeof(List<>).MakeGenericType(t)),
        (nameof(IReadOnlyCollection<int>), t => typeof(IReadOnlyCollection<>).MakeGenericType(t)),
        (nameof(ReadOnlyCollection<int>), t => typeof(ReadOnlyCollection<>).MakeGenericType(t)),
        (nameof(IReadOnlyList<int>), t => typeof(IReadOnlyList<>).MakeGenericType(t)),
        (nameof(MyList<int>), t => typeof(MyList<>).MakeGenericType(t)),
        (nameof(MyListWithCtor<int>), t => typeof(MyListWithCtor<>).MakeGenericType(t)),
    ];

    [XunitSerializable]
    public partial record TestCase(string Name, IDictionary Dictionary, [property: JsonIgnore] Func<Type, Type> Converter)
    {
        public override string ToString() => Name;
    }

    public static readonly TheoryData<TestCase> Cases = Dictionaries.Index()
        .CrossJoin(KvToColConverters)
        .Select(static m => new TestCase(m.Item2.Name + "_" + m.Item1.Index, m.Item1.Item, m.Item2.Converter))
        .ToTheoryData();

    private static void ReadTestGeneric<T, TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> raw)
        where T : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : notnull
    {
        var json = raw.ToJson<object>(); // use object converter to serialize IEnumerable<KeyValuePair<TKey, TValue>> to a dictionary
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
    public void ReadTest(TestCase testCase)
    {
        var (_, dic, converter) = testCase;

        var dicType = dic.GetType();
        var keyType = dicType.GenericTypeArguments[0];
        var valueType = dicType.GenericTypeArguments[1];
        var kvType = _kvRawType.MakeGenericType(keyType, valueType);
        var colType = converter(kvType);
        _method.MakeGenericMethod(colType, keyType, valueType)
            .Invoke(null, [dic]);
    }
}