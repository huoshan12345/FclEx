using FclEx.TestModels;

namespace FclEx.Extensions.BytesExtensions;

public class MarshalToTests
{
    private static readonly MethodInfo _marshalTo = typeof(MarshalToTests).GetRequiredMethod(nameof(MarshalTo));
    private static readonly MethodInfo _marshalToArray = typeof(MarshalToTests).GetRequiredMethod(nameof(MarshalToArray));

    public static readonly int[] IntArr = Enumerable.Range(1, 5).ToArray();

    public static readonly TheoryData<Array> ArrayCases = new()
    {
        IntArr.Select(m => m.CastTo<byte>()).ToArray(),
        IntArr.Select(m => m.CastTo<short>()).ToArray(),
        IntArr,
        IntArr.Select(m => m.CastTo<long>()).ToArray(),
        IntArr.Select(m => new MarshalableStruct
        {
            Int = m,
            Char = m.ToString()[0],
            Array = Enumerable.Repeat(m, 4).Select(x => x.CastTo<byte>()).ToArray(),
        }).ToArray(),
        IntArr.Select(m => new MarshalableClass
        {
            Int = m,
            Char = m.ToString()[0],
            Array = Enumerable.Repeat(m, 4).Select(x => x.CastTo<byte>()).ToArray(),
        }).ToArray(),
    };

    public static readonly TheoryData<object> ItemCases = new()
    {
        byte.MaxValue,
        short.MaxValue,
        int.MaxValue,
        long.MaxValue,
        new MarshalableStruct
        {
            Int = 99,
            Char = 'A',
            Array = [0x1, 0x2, 0x3, 0x4],
        },
        new MarshalableClass
        {
            Int = 99,
            Char = 'A',
            Array = [0x1, 0x2, 0x3, 0x4],
        },
    };

    private static void MarshalTo<T>(T item)
    {
        var bytes = ObjectHelper.MarshalToBytes(item);
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