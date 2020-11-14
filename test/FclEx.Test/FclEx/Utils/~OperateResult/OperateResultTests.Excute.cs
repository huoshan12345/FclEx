using System;
using Xunit;

namespace FclEx.Utils
{
    partial class OperateResultTests
    {
        [Fact]
        public void TestExcute()
        {
            var r = Operate.Excute(() => new object());

            Assert.True(r.Successful);
            Assert.NotNull(r.Result);
            Assert.NotEqual(default, r.Elapsed);
        }

        [Fact]
        public void TestExcuteError()
        {
            var r = Operate.Excute((Func<object>)(() => throw new SimpleException("")));
            Assert.True(!r.Successful);
            Assert.Null(r.Result);
            Assert.NotEqual(default, r.Elapsed);
            Assert.NotNull(r.Exception);
        }
    }
}
