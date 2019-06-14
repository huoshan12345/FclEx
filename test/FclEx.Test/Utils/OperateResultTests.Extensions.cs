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
        public void TestExcuteOkAfterOk()
        {
            var i = 0;
            var r = OperateResult.Excute(() => new object())
                .Ok((_, t) => ++i)
                .Ok((_, t) => ++i)
                .Error(e => ++i)
                .Error(e => ++i);

            Assert.True(r.Successful);
            Assert.NotNull(r.Result);
            Assert.NotEqual(default, r.Elapsed);
            Assert.Equal(2, i);
        }

        [Fact]
        public void TestExcuteErrorAfterError()
        {
            var i = 0;
            var r = OperateResult.Excute((Func<object>)(() => throw new SimpleException("")))
                .Ok((_, t) => ++i)
                .Ok((_, t) => ++i)
                .Error(e => ++i)
                .Error(e => ++i);

            Assert.True(!r.Successful);
            Assert.Null(r.Result);
            Assert.NotEqual(default, r.Elapsed);
            Assert.NotNull(r.Exception);
            Assert.Equal(2, i);
        }
    }
}
