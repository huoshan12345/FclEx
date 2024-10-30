using FclEx.TestModels;
using Xunit.Sdk;

namespace FclEx.Comparers;

public class BlittableEqualityComparerTests(ITestOutputHelper output)
{
    public static readonly IEnumerable<Type> ValueTypes = Types.BlittableTypes.Concat([
        typeof(MarshalableClass),
        typeof(MarshalableStruct)]);

    public static readonly IEnumerable<object[]> TypeCases = ValueTypes.Select(m => new object[] { m });

    private static readonly MethodInfo _equals = typeof(BlittableEqualityComparerTests).GetRequiredMethod(nameof(Equals));

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
        Assert.Equal<T>(x, x, MarshalToBytesEqualityComparer<T>.Instance);
        output.WriteLine(x);
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(DateTimeOffset))]
    [InlineData(typeof(TestRecord))]
    [InlineData(typeof(TestStruct))]
    [InlineData(typeof(TestRecordStruct))]
    public void Equals_NonBlittable_Test(Type type)
    {
        var ex = Assert.Throws<TargetInvocationException>(() => _equals.MakeGenericMethod(type).Invoke(this, null));
        var inner = Assert.IsType<ArgumentException>(ex.InnerException);
        Assert.Contains("cannot be marshaled as an unmanaged structure", inner.Message);
    }

    [Theory]
    [InlineData(typeof(Tuple<int>))]
    [InlineData(typeof(ValueTuple<int>))]
    public void Equals_Generic_Test(Type type)
    {
        var ex = Assert.Throws<TargetInvocationException>(() => _equals.MakeGenericMethod(type).Invoke(this, null));
        var innermost = Assert.IsType<ArgumentException>(ex.InnerException);
        Assert.Contains("The specified Type must not be a generic", innermost.Message);
    }
}