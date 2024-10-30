using FclEx.TestModels;

namespace FclEx.Extensions.BytesExtensions;

public class MarshalToTests
{
    private static readonly MethodInfo _marshalTo = typeof(MarshalToTests).GetRequiredMethod(nameof(MarshalTo));
    private static readonly MethodInfo _marshalToArray = typeof(MarshalToTests).GetRequiredMethod(nameof(MarshalToArray));

    public static int[] IntArr { get; } = Enumerable.Range(1, 5).ToArray();

    public static IEnumerable<object[]> ArrayCases { get; } = new object[]
    {
        IntArr.Select(m => m.CastTo<byte>()).ToArray(),
        IntArr.Select(m => m.CastTo<short>()).ToArray(),
        IntArr,
        IntArr.Select(m => m.CastTo<long>()).ToArray(),
        IntArr.Select(m => new BlittableStruct
        {
            Number = m,
            Char = m.ToString()[0],
            Arr = Enumerable.Repeat(m, 4).Select(x => x.CastTo<byte>()).ToArray(),
        }).ToArray(),
        IntArr.Select(m => new BlittableClass
        {
            Number = m,
            Char = m.ToString()[0],
            Arr = Enumerable.Repeat(m, 4).Select(x => x.CastTo<byte>()).ToArray(),
        }).ToArray(),
    }.Select(m => new[] { m }).ToArray();

    public static IEnumerable<object[]> ItemCases { get; } = new object[]
    {
        byte.MaxValue,
        short.MaxValue,
        int.MaxValue,
        long.MaxValue,
        new BlittableStruct
        {
            Number = 99,
            Char = 'A',
            Arr = [0x1, 0x2, 0x3, 0x4],
        },
        new BlittableClass
        {
            Number = 99,
            Char = 'A',
            Arr = [0x1, 0x2, 0x3, 0x4],
        },
    }.Select(m => new[] { m }).ToArray();

    private static void MarshalTo<T>(T item)
    {
        var bytes = item.MarshalToBytes();
        var actual = bytes.MarshalTo<T>();
        Assert.Equal(actual, item, MarshalToBytesEqualityComparer<T>.Instance);
    }

    private static void MarshalToArray<T>(T[] item)
    {
        var bytes = item.MarshalArrayToBytes();
        var actual = bytes.MarshalToArray<T>();
        Assert.True(actual.SequenceEqual(item, MarshalToBytesEqualityComparer<T>.Instance));
    }

    [Theory]
    [MemberData(nameof(ItemCases))]
    public void MarshalTo_Test(object item)
    {
        _marshalTo.MakeGenericMethod(item.GetType())
            .Invoke(null, [item]);
    }

    [Theory]
    [MemberData(nameof(ArrayCases))]
    public void MarshalToArray_Test(Array arr)
    {
        _marshalToArray.MakeGenericMethod(arr.GetValue(0)!.GetType())
            .Invoke(null, [arr]);
    }
}