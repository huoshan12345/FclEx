using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Utils;
using Xunit;

namespace FclEx.Test.Utils
{
    public partial class OperateResultTests
    {
        [Fact]
        public void ImplicitOperator_FromData()
        {
            const int expected = 1;
            var r = Test(expected);
            Assert.True(r.Successful);
            Assert.Equal(expected, r.Result);

            OperateResult<int> Test(int input)
            {
                return input;
            }
        }

        [Fact]
        public void ImplicitOperator_FromString()
        {
            var r = Test();
            Assert.False(r.Successful);
            Assert.IsType<SimpleException>(r.Exception);

            OperateResult<int> Test()
            {
                return "";
            }
        }

        [Fact]
        public void ImplicitOperator_FromException()
        {
            var ex = new ArgumentException();
            var r = Test(ex);
            Assert.False(r.Successful);
            Assert.IsType(ex.GetType(), r.Exception);

            OperateResult<int> Test(Exception e)
            {
                return ex;
            }
        }
    }
}
