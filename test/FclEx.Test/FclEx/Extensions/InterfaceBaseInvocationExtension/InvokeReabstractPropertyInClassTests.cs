using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FclEx.Extensions.InterfaceBaseInvocationExtension
{
    public class InvokeReabstractPropertyInClassTests
    {
        public interface I0
        {
            int Count => 1;
        }

        public interface I1 : I0
        {
            abstract int I0.Count { get; }
        }

        public interface I2 : I1
        {
            int I0.Count => 2;
        }

        public class WithI2 : I2
        {
            int I0.Count => this.BaseByDelegate<I2, int>(m => m.Count);
            public int Count => this.BaseByDelegate<I1, int>(m => m.Count);
        }

        [Fact]
        public void Test()
        {
            var c = new WithI2();
            var ex = Assert.Throws<InvalidOperationException>(() => c.Count);
            Assert.Contains("is abstract", ex.Message);

            Assert.Equal(2, ((I0)c).Count);
            Assert.Equal(2, ((I1)c).Count);
            Assert.Equal(2, ((I2)c).Count);
        }
    }
}
