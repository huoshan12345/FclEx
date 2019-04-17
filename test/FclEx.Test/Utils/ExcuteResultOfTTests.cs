using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FclEx.Utils;
using Xunit;

namespace FclEx.Test.Utils
{
    public class ExcuteResultOfTTests
    {
        [Fact]
        public void TestExcute()
        {
            var r = ExcuteResult.Excute(() => new object());
            Assert.True(r.Successful);
            Assert.NotNull(r.Result);
            Assert.NotEqual(default, r.Elapsed);
        }

        [Fact]
        public void TestExcuteError()
        {
            var r = ExcuteResult.Excute((Func<object>)(() => throw new SimpleException("")));
            Assert.True(!r.Successful);
            Assert.Null(r.Result);
            Assert.NotEqual(default, r.Elapsed);
            Assert.NotNull(r.Exception);
        }
    }
}
