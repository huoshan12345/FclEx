using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using FclEx.Extensions;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace FclEx.Abp.OrmLite
{
    public class OrmLiteConStrResolver : IOrmLiteConStrResolver, ISingletonDependency
    {
        private readonly ConcurrentDictionary<string, OrmLiteConStr> _conStrs;

        public OrmLiteConStrResolver(IOptions<AbpOrmLiteOptions> options)
        {
            _conStrs = options.Value.ConStrs.ToConcurrentDictionary(m => m.Name);
        }

        public IReadOnlyCollection<OrmLiteConStr> GetConStrs()
        {
            return _conStrs.Values.AsReadOnly();
        }

        public OrmLiteConStr Get(string name)
        {
            if (_conStrs.TryGetValue(name, out var con))
                return con;
            throw new KeyNotFoundException("Can not find ConStr named " + name);
        }

        public void Add(string name, OrmLiteConStr str)
        {
            _conStrs[name] = str;
        }

        public OrmLiteConStr GetOrAdd(string name, Func<string, OrmLiteConStr> func)
        {
            return _conStrs.GetOrAdd(name, func);
        }
    }
}
