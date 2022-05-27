using System;
using System.Reflection;
using Xunit;

namespace FclEx.Extensions.InterfaceBaseInvocationExtension
{
    public class InvokeGenericMethodTests
    {
        private interface IHasEmptyGenericMethod
        {
            string Method(int x, string y);
            string Method<T>(int x, string y);
            string Method<T>(T x, string y);
        }

        private interface IHasOverridedGenericMethod : IHasEmptyGenericMethod
        {
            string IHasEmptyGenericMethod.Method(int x, string y) => $"{nameof(Method)}({x}, {y})";
            string IHasEmptyGenericMethod.Method<T>(int x, string y) => $"{nameof(Method)}<{typeof(T).Name}>({x}, {y})";
            string IHasEmptyGenericMethod.Method<T>(T x, string y) => $"{nameof(Method)}<{typeof(T).Name}>({typeof(T).Name} {x}, {y})";
        }

        private class HasOverridedGenericMethod : IHasOverridedGenericMethod
        {
            public string Method(int x, string y) => throw new InvalidOperationException();
            public string Method<T>(int x, string y) => throw new InvalidOperationException();
            public string Method<T>(T x, string y) => throw new InvalidOperationException();
        }

        [Fact]
        public void Method_Invoke()
        {
            var obj = new HasOverridedGenericMethod();
            var result = obj.BaseByDynamicMethod<IHasOverridedGenericMethod, string>(m => m.Method(1, "a"));
            Assert.Equal("Method(1, a)", result);
        }

        [Fact]
        public void GenericMethod_Invoke()
        {
            var obj = new HasOverridedGenericMethod();
            var result = obj.BaseByDynamicMethod<IHasOverridedGenericMethod, string>(m => m.Method<string>(1, "a"));
            Assert.Equal("Method<String>(1, a)", result);
        }

        [Fact]
        public void GenericMethod_Invoke_AmbiguousMatch()
        {
            var obj = new HasOverridedGenericMethod();
            Assert.Throws<AmbiguousMatchException>(() => obj.BaseByDynamicMethod<IHasOverridedGenericMethod, string>(m => m.Method<int>(1, "a")));
        }

        [Fact]
        public void GenericMethod_WithGenericParameter_Invoke()
        {
            var obj = new HasOverridedGenericMethod();
            var result = obj.BaseByDynamicMethod<IHasOverridedGenericMethod, string>(m => m.Method<string>("a", "a"));
            Assert.Equal("Method<String>(String a, a)", result);
        }
    }
}
