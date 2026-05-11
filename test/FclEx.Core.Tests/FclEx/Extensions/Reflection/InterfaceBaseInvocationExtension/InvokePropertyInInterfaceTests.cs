#if NET6_0_OR_GREATER
namespace FclEx.Extensions.Reflection.InterfaceBaseInvocationExtension;

public class InvokePropertyInInterfaceTests
{
    public interface I0
    {
        int Count { get; }
    }

    public interface I1 : I0
    {
        int I0.Count => 1;
    }

    public interface I2 : I1
    {
        int I0.Count => this.BaseByFunctionPointer<I1, int>(m => m.Count) + 1;
    }

    public class WithI2 : I2;

    public class ImplI2CallI1 : I2
    {
        int I0.Count => this.BaseByFunctionPointer<I1, int>(m => m.Count) + 1;
        public int Count => this.BaseByFunctionPointer<I1, int>(m => m.Count) + 1;
    }

    public class ImplI2CallI2 : I2
    {
        int I0.Count => this.BaseByFunctionPointer<I2, int>(m => m.Count) + 1;
        public int Count => this.BaseByFunctionPointer<I2, int>(m => m.Count) + 1;
    }

    [Fact]
    public void Test()
    {
        var c = new WithI2();
        Assert.Equal(2, ((I0)c).Count);
        Assert.Equal(2, ((I1)c).Count);
        Assert.Equal(2, ((I2)c).Count);
    }

    [Fact]
    public void Implicit_Impl_Test()
    {
        var c = new ImplI2CallI1();
        Assert.Equal(2, c.Count);
    }

    [Fact]
    public void Explicit_Impl_Test()
    {
        var c = new ImplI2CallI1();
        Assert.Equal(2, ((I0)c).Count);
        Assert.Equal(2, ((I1)c).Count);
        Assert.Equal(2, ((I2)c).Count);
    }

    [Fact]
    public void Implicit_Impl_ChainedCall_Test()
    {
        var c = new ImplI2CallI2();
        Assert.Equal(3, c.Count);
    }

    [Fact]
    public void Explicit_Impl_ChainedCall_Test()
    {
        var c = new ImplI2CallI2();
        Assert.Equal(3, ((I0)c).Count);
        Assert.Equal(3, ((I1)c).Count);
        Assert.Equal(3, ((I2)c).Count);
    }
}
#endif