using FclEx.TestModels;

namespace FclEx.Comparers;

public class BytewiseEqualityComparerTests(ITestOutputHelper output)
{
    public static readonly IEnumerable<Type> ValueTypes = Types.BlittableTypes.Concat([
        typeof(ValueTuple<int>), // blittable but not marshalable
        typeof(DateTime), // non-blittable
        typeof(DateTimeOffset), // non-blittable
        typeof(ValueTuple<int, long, DateTimeOffset, DateTime>), // non-blittable
        typeof(string),
        typeof(TestStruct),
        typeof(TestRecord),
        typeof(TestRecordStruct),
        typeof(MarshalableClass),
        typeof(MarshalableStruct)]);

    public static readonly IEnumerable<object[]> TypeCases = ValueTypes.Select(m => new object[] { m });

    private static readonly MethodInfo _equals = typeof(BytewiseEqualityComparerTests).GetRequiredMethod(nameof(Equals));

    [Theory]
    [MemberData(nameof(TypeCases))]
    public void Equals_Test(Type type)
    {
        _equals.MakeGenericMethod(type).Invoke(this, null);
    }

    private void Equals<T>()
    {
        var random = new Random(0);
        var x = random.Next<T>();
        Assert.Equal(x, x, BytewiseEqualityComparer<T>.Instance);
        output.WriteLine(x);
    }
}