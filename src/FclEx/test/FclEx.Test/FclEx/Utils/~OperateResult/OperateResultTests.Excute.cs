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

            Assert.True(r.Success);
            Assert.NotNull(r.Value);
            Assert.NotEqual(default, r.Elapsed);
        }

        [Fact]
        public void TestExcuteError()
        {
            var r = Operate.Excute((Func<object>)(() => throw new SimpleException("")));
            Assert.True(!r.Success);
            Assert.Null(r.Value);
            Assert.NotEqual(default, r.Elapsed);
            Assert.NotNull(r.Exception);
        }
    }
}
