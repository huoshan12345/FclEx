namespace FclEx.Extensions.ByteExtensions;

public class UnmanagedTests
{
    private static readonly MethodInfo _methodOfSingle = typeof(UnmanagedTests).GetRequiredMethod(nameof(Single));
    private static readonly MethodInfo _methodOfArray = typeof(UnmanagedTests).GetRequiredMethod(nameof(Array));

    public static int[] IntArr { get; } = Enumerable.Range(1, 10).ToArray();

    public static IEnumerable<object[]> ArrayCase { get; } = new object[]
    {
        IntArr.Select(m => m.CastTo<byte>()).ToArray(),
        IntArr.Select(m => m.CastTo<short>()).ToArray(),
        IntArr,
        IntArr.Select(m => m.CastTo<long>()).ToArray(),
        IntArr.Select(m => new UnmanagedStruct
        {
            Number = m,
            Char = m.ToString()[0],
            Arr = Enumerable.Repeat(m, 4).Select(x => x.CastTo<byte>()).ToArray(),
        }).ToArray()
    }.Select(m => new[] { m }).ToArray();

    public static IEnumerable<object[]> SingleCase { get; } = new object[]
    {
        byte.MaxValue,
        short.MaxValue,
        int.MaxValue,
        long.MaxValue,
        new UnmanagedStruct
        {
            Number = 99,
            Char = 'A',
            Arr = Enumerable.Range(1, 4).Select(m => m.CastTo<byte>()).ToArray(),
        },
    }.Select(m => new[] { m }).ToArray();

    private static void Single<T>(T item) where T : struct
    {
        var bytes = item.ToBytes();
        var actual = bytes.ToStructure<T>();
        Assert.Equal(actual, item);
    }

    private static void Array<T>(T[] item) where T : struct
    {
        var bytes = item.ToBytes();
        var actual = bytes.ToStructures<T>();
        Assert.True(actual.SequenceEqual(item));
    }

    [Theory]
    [MemberData(nameof(SingleCase))]
    public void SingleTest(object item)
    {
        _methodOfSingle.MakeGenericMethod(item.GetType())
            .Invoke(null, new object[] { item });
    }

    [Theory]
    [MemberData(nameof(ArrayCase))]
    public void ArrayTest(Array arr)
    {
        _methodOfArray.MakeGenericMethod(arr.GetValue(0)!.GetType())
            .Invoke(null, new object[] { arr });
    }
}