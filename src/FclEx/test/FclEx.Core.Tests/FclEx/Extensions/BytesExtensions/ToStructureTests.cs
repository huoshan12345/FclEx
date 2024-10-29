using FclEx.TestModels;

namespace FclEx.Extensions.BytesExtensions;

public class ToStructureTests
{
    private static readonly MethodInfo _methodOfSingle = typeof(ToStructureTests).GetRequiredMethod(nameof(ToStructure));
    private static readonly MethodInfo _methodOfArray = typeof(ToStructureTests).GetRequiredMethod(nameof(ToStructures));

    public static int[] IntArr { get; } = Enumerable.Range(1, 5).ToArray();

    public static IEnumerable<object[]> ArrayCases { get; } = new object[]
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
        }).ToArray(),
    }.Select(m => new[] { m }).ToArray();

    public static IEnumerable<object[]> ItemCases { get; } = new object[]
    {
        byte.MaxValue,
        short.MaxValue,
        int.MaxValue,
        long.MaxValue,
        new UnmanagedStruct
        {
            Number = 99,
            Char = 'A',
            Arr = [0x1, 0x2, 0x3, 0x4],
        },
    }.Select(m => new[] { m }).ToArray();

    private static void ToStructure<T>(T item) where T : struct
    {
        var bytes = item.ToBytes();
        var actual = bytes.ToStructure<T>();
        Assert.Equal(actual, item);
    }

    private static void ToStructures<T>(T[] item) where T : struct
    {
        var bytes = item.ToBytes();
        var actual = bytes.ToStructures<T>();
        Assert.True(actual.SequenceEqual(item));
    }

    [Theory]
    [MemberData(nameof(ItemCases))]
    public void ToStructure_Test(object item)
    {
        _methodOfSingle.MakeGenericMethod(item.GetType())
            .Invoke(null, [item]);
    }

    [Theory]
    [MemberData(nameof(ArrayCases))]
    public void ToStructures_Test(Array arr)
    {
        _methodOfArray.MakeGenericMethod(arr.GetValue(0)!.GetType())
            .Invoke(null, [arr]);
    }
}