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
        public void Inherit_Test()
        {
            var c = new InheritI2();

            Assert.Equal(1, c.BaseByDelegate<I1, int>(m => m.Compute(0)));
            Assert.Equal(2, c.BaseByDelegate<I2, int>(m => m.Compute(0)));

            c.BaseByDelegate<I1>(m => m.Call());
            c.BaseByDelegate<I2>(m => m.Call());
        }
    }
}
