using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FclEx.Extensions.InterfaceBaseInvocationExtension
{
    public class InvokeMethodInClassTests
    {
        public interface I0
        {
            int Compute(int number);
        }

        public interface I1 : I0
        {
            int I0.Compute(int number) => number + 1;
        }

        public interface I2 : I1
        {
            int I0.Compute(int number) => number + 2;
        }

        public class InheritI2 : I2
        {
        }

        [Fact]
        public void AbstractMethod_Test()
        {
            var c = new InheritI2();
            Assert.Throws<InvalidOperationException>(() => c.BaseByDynamicMethod<I0, int>(m => m.Compute(0)));
        }

        [Fact]
        public void Inherit_Test()
        {
            var c = new InheritI2();
            Assert.Equal(1, c.BaseByDynamicMethod<I1, int>(m => m.Compute(0)));
            Assert.Equal(2, c.BaseByDynamicMethod<I2, int>(m => m.Compute(0)));
        }
    }
}
