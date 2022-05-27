using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace LightInject
{
    public class LightInjectHelperTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Dispose_Test(bool useAop)
        {
            var container = LightInjectHelper.CreateContainer(new LightInjectOptions { UseAop = useAop });
            container.Dispose();
        }
    }
}
