namespace FclEx.Extensions.TypeExtensions;

public class LongNameTests
{
    public class Tester
    {
        public void Test<T>() { }
    }

    [Fact]
    public void TypeGenericParameter_Test()
    {
        var type = typeof(List<>).GetGenericArguments().First();
        Assert.True(type.IsGenericParameter);
        var name = type.LongName();
        Assert.Equal("System.Collections.Generic.List<>.T", name);
    }

    [Fact]
    public void MethodGenericParameter_Test()
    {
        var method = typeof(Tester).GetMethod(nameof(Tester.Test));
        Assert.NotNull(method);

        var type = method.GetGenericArguments().First();
        Assert.True(type.IsGenericParameter);
        var name = type.LongName();
        Assert.Equal("FclEx.Extensions.TypeExtensions.LongNameTests.Tester.T", name);
    }
}