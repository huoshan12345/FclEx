using FclEx.TestModels;
using Xunit.Sdk;

namespace System.Collections.Generic;

public class MarshalToBytesEqualityComparerTests(ITestOutputHelper output)
{
    public static readonly IEnumerable<Type> ValueTypes = Types.BlittableTypes.Concat([
        typeof(decimal),
        typeof(TimeSpan),
        typeof(Guid),
        typeof(DateOnly),
        typeof(TimeOnly),
        typeof(MarshalableClass),
        typeof(MarshalableStruct)]);

    public static readonly IEnumerable<object[]> TypeCases = ValueTypes.Select(m => new object[] { m });

    private static readonly MethodInfo _equals = typeof(MarshalToBytesEqualityComparerTests).GetRequiredMethod(nameof(Equals));

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
    //[InlineData(typeof(string))]
    //[InlineData(typeof(DateTime))]
    [InlineData(typeof(DateTimeOffset))]
    //[InlineData(typeof(CommonRecord))]
    //[InlineData(typeof(CommonStruct))]
    //[InlineData(typeof(CommonRecordStruct))]
    public void Equals_AutoLayout_Test(Type type)
    {
        var ex = Assert.Throws<TargetInvocationException>(() => _equals.MakeGenericMethod(type).Invoke(this, null));
        var inner = Assert.IsType<EqualException>(ex.InnerException);
        var innermost = Assert.IsType<ArgumentException>(inner.InnerException);
        output.WriteLine(innermost.Message);
        Assert.Contains("is not marshalable because it is auto layout.", innermost.Message);
    }

    [Theory]
    [InlineData(typeof(Tuple<int>))]
    [InlineData(typeof(ValueTuple<int>))]
    public void Equals_Generic_Test(Type type)
    {
        var ex = Assert.Throws<TargetInvocationException>(() => _equals.MakeGenericMethod(type).Invoke(this, null));
        var inner = Assert.IsType<EqualException>(ex.InnerException);
        var innermost = Assert.IsType<ArgumentException>(inner.InnerException);
        output.WriteLine(innermost.Message);
        Assert.Contains("is not marshalable because it is generic", innermost.Message);
    }
}