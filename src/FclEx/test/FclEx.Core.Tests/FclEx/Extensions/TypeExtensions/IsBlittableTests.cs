using FclEx.TestModels;

namespace FclEx.Extensions.TypeExtensions;

public class IsBlittableTests
{
    public static readonly IEnumerable<object[]> TestCases = Types.BlittableTypes
        .Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(TestCases))]
    public void IsBlittable_BlittableType_Test(Type type)
    {
        var result = type.IsBlittable(out var ex);
        Assert.True(result, ex?.ToString());
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void IsBlittable_ArrayOfBlittableType_Test(Type type)
    {
        var result = type.MakeArrayType().IsBlittable(out var ex);
        Assert.True(result, ex?.ToString());
    }

    [Theory]
    [InlineData(typeof(BlittableStruct))]
    [InlineData(typeof(BlittableClass))]
    public void IsBlittable_NonAutoLayout_Test(Type type)
    {
        var result = type.IsBlittable(out var ex);
        Assert.True(result, ex?.ToString());
    }

    [Theory]
    [InlineData(typeof(MarshalableStruct))]
    [InlineData(typeof(MarshalableClass))]
    public void IsBlittable_NonAutoLayout_ContainsNonBlittable_Test(Type type)
    {
        var result = type.IsBlittable(out var ex);
        Assert.False(result);
        Assert.NotNull(ex);
        Assert.Contains("", ex.Message);
    }

    [Theory]
    [InlineData(typeof(TestStruct))]
    [InlineData(typeof(TestClass))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(DateTimeOffset))]
    public void IsBlittable_AutoLayout_Test(Type type)
    {
        var result = type.IsBlittable(out var ex);
        Assert.False(result);
        Assert.NotNull(ex);
        Assert.Contains("", ex.Message);
    }
}