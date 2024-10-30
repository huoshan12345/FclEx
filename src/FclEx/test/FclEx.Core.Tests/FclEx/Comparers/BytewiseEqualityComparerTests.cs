using FclEx.TestModels;

namespace FclEx.Comparers;

public class BytewiseEqualityComparerTests(ITestOutputHelper output)
{
    public static readonly IEnumerable<Type> ValueTypes = Types.CommonValueTypes.Concat([
        typeof(string),
        typeof(TestStruct),
        typeof(TestRecord),
        typeof(TestRecordStruct),
        typeof(BlittableClass),
        typeof(BlittableStruct)]);

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
        //var random = new Random(0);
        //var x = random.Next<T>();
        var x = default(T);
        Assert.Equal<T>(x, x, BytewiseEqualityComparer<T>.Instance);
        output.WriteLine(x);
    }
}