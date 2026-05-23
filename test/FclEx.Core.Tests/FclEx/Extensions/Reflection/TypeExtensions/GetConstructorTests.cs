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
    public void GetDefaultConstructor_ShouldReturnNull_WhenNoDefaultConstructorExists()
    {
        var result = typeof(NoDefaultCtorClass).GetDefaultConstructor();

        Assert.Null(result);
    }

    [Fact]
    public void GetDefaultConstructor_ShouldReturnConstructor_WhenDefaultConstructorExists()
    {
        var result = typeof(TestClass).GetDefaultConstructor();

        Assert.NotNull(result);
        Assert.Equal(0, result.GetParameters().Length);
    }

    [Fact]
    public void GetDefaultConstructor_ShouldReturnConstructor_WhenPrivateDefaultConstructorExists()
    {
        var result = typeof(PrivateDefaultCtorClass).GetDefaultConstructor();

        Assert.NotNull(result);
        Assert.Equal(0, result.GetParameters().Length);
    }

    [Fact]
    public void GetRequiredDefaultConstructor_ShouldThrowArgumentException_WhenNoDefaultConstructorExists()
    {
        var exception = Assert.Throws<ArgumentException>(() => typeof(NoDefaultCtorClass).GetRequiredDefaultConstructor());

        Assert.Contains($"The type '{typeof(GetConstructorTests).FullName}.{nameof(NoDefaultCtorClass)}' does not have a default constructor.", exception.Message);
    }

    [Fact]
    public void GetRequiredDefaultConstructor_ShouldReturnConstructor_WhenDefaultConstructorExists()
    {
        var result = typeof(TestClass).GetRequiredDefaultConstructor();

        Assert.NotNull(result);
        Assert.Equal(0, result.GetParameters().Length);
    }
}
