using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Utils;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Test.Utils
{
    public class SimpleExceptionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public SimpleExceptionTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void StackTrace_Test()
        {
            var ex = new SimpleException("test", new Exception("inner"));
            _outputHelper.WriteLine(ex.ToString());
        }
    }
}
