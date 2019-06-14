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
        public void TestExcute()
        {
            var r = OperateResult.Excute(() => new object());

            Assert.True(r.Successful);
            Assert.NotNull(r.Result);
            Assert.NotEqual(default, r.Elapsed);
        }

        [Fact]
        public void TestExcuteError()
        {
            var r = OperateResult.Excute((Func<object>)(() => throw new SimpleException("")));
            Assert.True(!r.Successful);
            Assert.Null(r.Result);
            Assert.NotEqual(default, r.Elapsed);
            Assert.NotNull(r.Exception);
        }
    }
}
