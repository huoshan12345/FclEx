#if NET6_0_OR_GREATER
namespace FclEx.Extensions.Reflection.InterfaceBaseInvocationExtension;

public class InvokeGenericMethodTests
{
    private interface IHasEmptyGenericMethod
    {
        string Method(int x, string y);
        string Method<T>(int x, string y);
        string Method<T>(T x, string y);
    }

    private interface IHasOverrideGenericMethod : IHasEmptyGenericMethod
    {
        string IHasEmptyGenericMethod.Method(int x, string y) => $"{nameof(Method)}({x}, {y})";
        string IHasEmptyGenericMethod.Method<T>(int x, string y) => $"{nameof(Method)}<{typeof(T).Name}>({x}, {y})";
        string IHasEmptyGenericMethod.Method<T>(T x, string y) => $"{nameof(Method)}<{typeof(T).Name}>({typeof(T).Name} {x}, {y})";
    }

    private class HasOverrideGenericMethod : IHasOverrideGenericMethod
    {
        public string Method(int x, string y) => throw new InvalidOperationException();
        public string Method<T>(int x, string y) => throw new InvalidOperationException();
        public string Method<T>(T x, string y) => throw new InvalidOperationException();
    }

    [Fact]
    public void Method_Invoke()
    {
        var obj = new HasOverrideGenericMethod();
        var result = obj.BaseByDynamicMethod<IHasOverrideGenericMethod, string>(m => m.Method(1, "a"));
        Assert.Equal("Method(1, a)", result);
    }

    [Fact]
    public void GenericMethod_Invoke()
    {
        var obj = new HasOverrideGenericMethod();
        var result = obj.BaseByDynamicMethod<IHasOverrideGenericMethod, string>(m => m.Method<string>(1, "a"));
        Assert.Equal("Method<String>(1, a)", result);
    }

    [Fact]
    public void GenericMethod_Invoke_AmbiguousMatch()
    {
        var obj = new HasOverrideGenericMethod();
        Assert.Throws<AmbiguousMatchException>(() => obj.BaseByDynamicMethod<IHasOverrideGenericMethod, string>(m => m.Method<int>(1, "a")));
    }

    [Fact]
    public void GenericMethod_WithGenericParameter_Invoke()
    {
        var obj = new HasOverrideGenericMethod();
        var result = obj.BaseByDynamicMethod<IHasOverrideGenericMethod, string>(m => m.Method<string>("a", "a"));
        Assert.Equal("Method<String>(String a, a)", result);
    }
}
#endif