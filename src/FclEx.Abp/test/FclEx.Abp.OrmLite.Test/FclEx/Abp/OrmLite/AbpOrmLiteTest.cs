using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Abp.Xunit;
using Xunit.Abstractions;

namespace FclEx.Abp.OrmLite
{
    public class AbpOrmLiteTest : AbpTests<AbpOrmLiteTestModule>
    {
        public AbpOrmLiteTest(ITestOutputHelper output, Action<AbpTestsOptions> action = null) : base(output, action)
        {
        }
    }
}
