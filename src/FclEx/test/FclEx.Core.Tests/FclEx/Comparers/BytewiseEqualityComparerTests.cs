using FclEx.TestModels;

namespace FclEx.Comparers;

public class BytewiseEqualityComparerTests(ITestOutputHelper output)
{
    public static readonly Type[] Types =
    [
        typeof(bool),
        typeof(char),
        typeof(sbyte),
        typeof(byte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(DateTime),
        typeof(TimeSpan),
        typeof(Guid),
        typeof(DateTimeOffset),
        typeof(DateOnly),
        typeof(TimeOnly),
        typeof(IntPtr),
        typeof(UIntPtr),
        typeof(ValueTuple<int>),
        typeof(ValueTuple<int, long, DateTimeOffset, DateTime>),
        typeof(UnmanagedStruct),
    ];

    public static readonly IEnumerable<object[]> TypeCases = Types.Select(m => new object[] { m });

    private static readonly MethodInfo _equals = typeof(BytewiseEqualityComparerTests).GetRequiredMethod(nameof(Equals));

    [Theory]
    [MemberData(nameof(TypeCases))]
    public void Equals_Test(Type type)
    {
        _equals.MakeGenericMethod(type).Invoke(this, null);
    }

    private void Equals<T>() where T : struct
    {
        //var random = new Random(0);
        //var x = random.Next<T>();
        var x = default(T);
        Assert.Equal(x, x, BytewiseEqualityComparer<T>.Instance);
        output.WriteLine(x.ToString());
    }
}

public struct Node
{
    public Node[] Nodes { get; set; }
}