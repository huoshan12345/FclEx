using System;
using System.Collections.Generic;
using System.Text;


namespace FclEx.Abp.Caching.Configuration
{
    public interface IAbpCacheReadOnlyOptions
    {
        string GlobalPrefix { get; }
        char? Separator { get; }
        TimeSpan DefaultExpiration { get; }
    }
}
