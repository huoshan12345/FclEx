using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FclEx.Extensions.InterfaceBaseInvocationExtension
{
    public class InvokeGenericInterfaceMethodTests
    {
        private interface IHasEmptyGenericMethod<in T>
        {
            string Method(int x, string y);
            string Method(T x, string y);
            string Method<TParameter>(int x, string y);
            string Method<TParameter>(int x, TParameter y);
            string Method<TParameter>(T x, TParameter y);
        }

        private interface IHasOverridedGenericMethod<in T> : IHasEmptyGenericMethod<T>
        {
            string IHasEmptyGenericMethod<T>.Method(int x, string y) => $"{nameof(Method)}({x}, {y})";
            string IHasEmptyGenericMethod<T>.Method(T x, string y) => $"{nameof(Method)}({typeof(T).Name} {x}, {y})";
            string IHasEmptyGenericMethod<T>.Method<TParameter>(int x, string y) => $"{nameof(Method)}<{typeof(TParameter).Name}>({x}, {y})";
            string IHasEmptyGenericMethod<T>.Method<TParameter>(int x, TParameter y) => $"{nameof(Method)}<{typeof(TParameter).Name}>({x}, {typeof(TParameter).Name} {y})";
            string IHasEmptyGenericMethod<T>.Method<TParameter>(T x, TParameter y) => $"{nameof(Method)}<{typeof(TParameter).Name}>({typeof(T).Name} {x}, {typeof(TParameter).Name} {y})";
        }

        private class HasOverridedGenericMethod<T> : IHasOverridedGenericMethod<T>
        {
            string IHasEmptyGenericMethod<T>.Method(int x, string y) => throw new InvalidOperationException();
            string IHasEmptyGenericMethod<T>.Method(T x, string y) => throw new InvalidOperationException();
            string IHasEmptyGenericMethod<T>.Method<TParameter>(int x, string y) => throw new InvalidOperationException();
            string IHasEmptyGenericMethod<T>.Method<TParameter>(int x, TParameter y) => throw new InvalidOperationException();
            string IHasEmptyGenericMethod<T>.Method<TParameter>(T x, TParameter y) => throw new InvalidOperationException();
        }

        [Fact]
        public void Method_Invoke()
        {
            var obj = new HasOverridedGenericMethod<double>();
            var result = obj.BaseByDynamicMethod<IHasOverridedGenericMethod<double>, string>(m => m.Method(1, "a"));
            Assert.Equal("Method(1, a)", result);
        }

        [Fact]
        public void Method_WithGenericParameter_Invoke()
        {
            var obj = new HasOverridedGenericMethod<string>();
            var result = obj.BaseByDynamicMethod<IHasOverridedGenericMethod<string>, string>(m => m.Method("a", "a"));
            Assert.Equal("Method(String a, a)", result);
        }

        [Fact]
        public void GenericMethod_Invoke()
        {
            var obj = new HasOverridedGenericMethod<double>();
            var result = obj.BaseByDynamicMethod<IHasOverridedGenericMethod<double>, string>(m => m.Method<long>(1, "a"));
            Assert.Equal("Method<Int64>(1, a)", result);
        }

        [Fact]
        public void GenericMethod_WithOneGenericParameter_Invoke()
        {
            var obj = new HasOverridedGenericMethod<double>();
            var result = obj.BaseByDynamicMethod<IHasOverridedGenericMethod<double>, string>(m => m.Method<long>(1, 1));
            Assert.Equal("Method<Int64>(1, Int64 1)", result);
        }

        [Fact]
        public void GenericMethod_WithTwoGenericParameter_Invoke()
        {
            var obj = new HasOverridedGenericMethod<double>();
            var result = obj.BaseByDynamicMethod<IHasOverridedGenericMethod<double>, string>(m => m.Method<long>(1.0, 1));
            Assert.Equal("Method<Int64>(Double 1, Int64 1)", result);
        }
    }
}
