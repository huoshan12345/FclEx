using FclEx.TestModels;

namespace FclEx.Extensions.BytesExtensions;

public class ToBlittableTests
{
    private static readonly MethodInfo _methodOfSingle = typeof(ToBlittableTests).GetRequiredMethod(nameof(ToBlittable));
    private static readonly MethodInfo _methodOfArray = typeof(ToBlittableTests).GetRequiredMethod(nameof(ToBlittableArray));

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

    private static void ToBlittable<T>(T item)
    {
        var bytes = item.BlittableToBytes();
        var actual = bytes.ToBlittable<T>();
        Assert.Equal(actual, item, BlittableEqualityComparer<T>.Instance);
    }

    private static void ToBlittableArray<T>(T[] item)
    {
        var bytes = item.BlittableArrayToBytes();
        var actual = bytes.ToBlittableArray<T>();
        Assert.True(actual.SequenceEqual(item, BlittableEqualityComparer<T>.Instance));
    }

    [Theory]
    [MemberData(nameof(ItemCases))]
    public void ToBlittable_Test(object item)
    {
        _methodOfSingle.MakeGenericMethod(item.GetType())
            .Invoke(null, [item]);
    }

    [Theory]
    [MemberData(nameof(ArrayCases))]
    public void ToBlittableArray_Test(Array arr)
    {
        _methodOfArray.MakeGenericMethod(arr.GetValue(0)!.GetType())
            .Invoke(null, [arr]);
    }
}