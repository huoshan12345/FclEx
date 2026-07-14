namespace FclEx.Extensions.Reflection.TypeExtensions;

public class GetConstructorTests
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TestClass
    {
        public TestClass() { }
        public TestClass(int x) { }
    }

    public class NoDefaultCtorClass
    {
        public NoDefaultCtorClass(int x) { }
    }

    public class PrivateDefaultCtorClass
    {
        private PrivateDefaultCtorClass() { }
    }

    [Fact]
    public void GetParameterlessConstructor_ShouldReturnNull_WhenNoParameterlessConstructorExists()
    {
        var result = typeof(NoDefaultCtorClass).GetParameterlessConstructor();

        Assert.Null(result);
    }

    [Fact]
    public void GetParameterlessConstructor_ShouldReturnConstructor_WhenParameterlessConstructorExists()
    {
        var result = typeof(TestClass).GetParameterlessConstructor();

        Assert.NotNull(result);
        Assert.Equal(0, result.GetParameters().Length);
    }

    [Fact]
    public void GetParameterlessConstructor_ShouldReturnConstructor_WhenPrivateParameterlessConstructorExists()
    {
        var result = typeof(PrivateDefaultCtorClass).GetParameterlessConstructor();

        Assert.NotNull(result);
        Assert.Equal(0, result.GetParameters().Length);
    }

    [Fact]
    public void GetRequiredParameterlessConstructor_ShouldThrowArgumentException_WhenNoParameterlessConstructorExists()
    {
        var exception = Assert.Throws<ArgumentException>(() => typeof(NoDefaultCtorClass).GetRequiredParameterlessConstructor());

        Assert.Contains($"The type '{typeof(GetConstructorTests).FullName}.{nameof(NoDefaultCtorClass)}' does not have a parameterless constructor.", exception.Message);
    }

    [Fact]
    public void GetRequiredParameterlessConstructor_ShouldReturnConstructor_WhenParameterlessConstructorExists()
    {
        var result = typeof(TestClass).GetRequiredParameterlessConstructor();

        Assert.NotNull(result);
        Assert.Equal(0, result.GetParameters().Length);
    }
}
