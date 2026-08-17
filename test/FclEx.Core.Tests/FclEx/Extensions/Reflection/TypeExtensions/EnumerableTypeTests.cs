namespace FclEx.Extensions.Reflection.TypeExtensions;

public class EnumerableTypeTests
{
    public static IEnumerable<(object, Type)> Values { get; } =
    [
        (new[]{1}, typeof(int)),
        (new List<int>(), typeof(int)),
        (Enumerable.Range(1, 2), typeof(int)),
        (new Dictionary<string, int>(), typeof(KeyValuePair<string, int>)),
    ];
    public static IEnumerable<(Type, Type?)> Types { get; } =
    [
        (typeof(string), typeof(char)),
        (typeof(IEnumerable), typeof(object)),
        (typeof(IEnumerable<>), typeof(IEnumerable<>).GetTypeInfo().GenericTypeParameters.First()),
        (typeof(IEnumerable<int>), typeof(int)),
        (typeof(IEmptyEnumerable), typeof(object)),
        (typeof(IMyEnumerable<>), typeof(IMyEnumerable<>).GetTypeInfo().GenericTypeParameters.First()),
        (typeof(IMyEnumerable<int>), typeof(int)),
    ];

    public static readonly TheoryData<Type, Type?> Cases = Values
        .Select(m => (m.Item1.GetType(), (Type?)m.Item2))
        .Concat(Types)
        .Select(m => (m.Item1, m.Item2))
        .ToTheoryData();

    [Theory]
    [MemberData(nameof(Cases))]
    public void EnumerableElementType_Test(Type type, Type? expected)
    {
        var elementType = type.EnumerableElementType();
        Assert.Equal(expected, elementType);
    }

    [Fact]
    public void EnumerableElementTypes_MultipleGenericEnumerableInterfaces_ReturnsAllTypes()
    {
        var elementTypes = typeof(IMultipleEnumerable).EnumerableElementTypes();

        Assert.Equal(2, elementTypes.Count);
        Assert.Contains(typeof(int), elementTypes);
        Assert.Contains(typeof(string), elementTypes);
    }

    [Fact]
    public void EnumerableElementType_MultipleGenericEnumerableInterfaces_ThrowsAmbiguousMatchException()
    {
        Assert.Throws<AmbiguousMatchException>(() => typeof(IMultipleEnumerable).EnumerableElementType());
    }

    private interface IEmptyEnumerable : IEnumerable<object>;

    private interface IMyEnumerable<out T> : IEnumerable<T>;
    private interface IMultipleEnumerable : IEnumerable<int>, IEnumerable<string>;
}
