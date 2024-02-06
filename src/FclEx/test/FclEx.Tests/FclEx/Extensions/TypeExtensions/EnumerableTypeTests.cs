namespace FclEx.Extensions.TypeExtensions;

public class EnumerableTypeTests
{
    public static IEnumerable<(object, Type)> Values { get; } = new (object, Type)[]
    {
        (new[]{1}, typeof(int)),
        (new List<int>(), typeof(int)),
        (Enumerable.Range(1, 2), typeof(int)),
        (new Dictionary<string, int>(), typeof(KeyValuePair<string, int>))
    };
    public static IEnumerable<(Type, Type?)> Types { get; } = new[]
    {
        (typeof(string), typeof(char)),
        (typeof(IEnumerable), null),
        (typeof(IEnumerable<>), typeof(IEnumerable<>).GetTypeInfo().GenericTypeParameters.First()),
        (typeof(IEnumerable<int>), typeof(int)),
        (typeof(IEmptyEnumerable), typeof(object)),
        (typeof(IMyEnumerable<>), typeof(IMyEnumerable<>).GetTypeInfo().GenericTypeParameters.First()),
        (typeof(IMyEnumerable<int>), typeof(int)),
    };

    public static IEnumerable<object?[]> Cases = Values
        .Select(m => (m.Item1.GetType(), (Type?)m.Item2))
        .Concat(Types)
        .Select(m => new object?[] { m.Item1, m.Item2 });

    [Theory]
    [MemberData(nameof(Cases))]
    public void Value_Test(Type type, Type expected)
    {
        var t = type.EnumerableType();
        Assert.Equal(expected, t);
    }

    private interface IEmptyEnumerable : IEnumerable<object>
    {

    }

    private interface IMyEnumerable<out T> : IEnumerable<T>
    {

    }
}