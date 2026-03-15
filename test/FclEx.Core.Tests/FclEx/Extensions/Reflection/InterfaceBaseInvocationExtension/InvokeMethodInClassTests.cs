namespace FclEx.Extensions.Reflection.InterfaceBaseInvocationExtension;

public class InvokeMethodInClassTests
{
    public interface I0
    {
        int Compute(int number);
        void Call();
    }

    public interface I1 : I0
    {
        int I0.Compute(int number) => number + 1;
        void I0.Call() => Console.WriteLine(nameof(I1) + "." + nameof(Call));
    }

    public interface I2 : I1
    {
        int I0.Compute(int number) => number + 2;
        void I0.Call() => Console.WriteLine(nameof(I2) + "." + nameof(Call));
    }

    public class InheritI2 : I2
    {
        public int Compute(int number) => throw new NotImplementedException();
        public void Call() => throw new NotImplementedException();
    }

    [Fact]
    public void AbstractMethod_Test()
    {
        var c = new InheritI2();
        Assert.Throws<InvalidOperationException>(() => c.BaseByDynamicMethod<I0, int>(m => m.Compute(0)));
        Assert.Throws<InvalidOperationException>(() => c.BaseByDynamicMethod<I0>(m => m.Call()));
    }

    [Fact]
    public void Inherit_FunctionPointer_Test()
    {
        var c = new InheritI2();

        Assert.Equal(1, c.BaseByFunctionPointer<I1, int>(m => m.Compute(0)));
        Assert.Equal(2, c.BaseByFunctionPointer<I2, int>(m => m.Compute(0)));

        c.BaseByFunctionPointer<I1>(m => m.Call());
        c.BaseByFunctionPointer<I2>(m => m.Call());
    }

    [Fact]
    public void Inherit_DynamicMethod_Test()
    {
        var c = new InheritI2();

        Assert.Equal(1, c.BaseByDynamicMethod<I1, int>(m => m.Compute(0)));
        Assert.Equal(2, c.BaseByDynamicMethod<I2, int>(m => m.Compute(0)));

        c.BaseByDynamicMethod<I1>(m => m.Call());
        c.BaseByDynamicMethod<I2>(m => m.Call());
    }

    [Fact]
    public void MethodInLambda_FunctionPointer_ShouldThrow()
    {
        var c = new InheritI2();
        Assert.ThrowsAny<InvalidOperationException>(() =>
            c.BaseByFunctionPointer<I1, int>(m => Operation.Execute(() => m.Compute(0)).Unwrap()));
    }

    [Fact]
    public void MethodInLambda_DynamicMethod_ShouldThrow()
    {
        var c = new InheritI2();
        Assert.ThrowsAny<InvalidOperationException>(() =>
            c.BaseByDynamicMethod<I1, int>(m => Operation.Execute(() => m.Compute(0)).Unwrap()));
    }
}