using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Modularity;

namespace FclEx.Abp.AspNetCore
{
    [DependsOn(typeof(FclExAbpModule))]
    public class FclExAbpAspNetCoreModule : AbpModule
    {
    }
}
