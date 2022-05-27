using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Abp.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace FclEx.Abp.DependencyInjection
{
    public class GenericInterfaceConventionalRegistrarTests : AbpTests<AbpTestModule>
    {
        public GenericInterfaceConventionalRegistrarTests(ITestOutputHelper output, Action<AbpTestsOptions> action = null) 
            : base(output, action)
        {
        }
    }
}
