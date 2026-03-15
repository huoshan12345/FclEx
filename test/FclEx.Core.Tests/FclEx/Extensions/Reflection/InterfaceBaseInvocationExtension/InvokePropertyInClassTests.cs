namespace FclEx.Extensions.Reflection.InterfaceBaseInvocationExtension;

public class InvokePropertyInClassTests
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
        int I0.Count => 2;
    }

    public class WithI2 : I2
    {
        int I0.Count => this.BaseByFunctionPointer<I2, int>(m => m.Count);
        public int Count => this.BaseByFunctionPointer<I1, int>(m => m.Count);
    }

    [Fact]
    public void Implicit_Impl_Test()
    {
        var c = new WithI2();
        Assert.Equal(1, c.Count);
    }

    [Fact]
    public void Explicit_Impl_Test()
    {
        var c = new WithI2();
        Assert.Equal(2, ((I0)c).Count);
        Assert.Equal(2, ((I1)c).Count);
        Assert.Equal(2, ((I2)c).Count);
    }
}