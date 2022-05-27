using System;
using System.Reflection;

namespace FclEx.Abp.Caching.Configuration
{
    public class CacheOptions
    {
        internal CacheOptions(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public bool OnlyUseLowerCase { get; set; } = true;
        public bool UsePrefix { get; set; } = true;
        public bool UseGlobalPrefix { get; set; } = false;
        public TimeSpan? DefaultExpiration { get; set; }
    }
}
