using System;
using System.Collections.Generic;

namespace FclEx.Abp.OrmLite
{
    public interface IOrmLiteConStrResolver
    {
        IReadOnlyCollection<OrmLiteConStr> GetConStrs();
        OrmLiteConStr Get(string name);
        void Add(string name, OrmLiteConStr str);
        OrmLiteConStr GetOrAdd(string name, Func<string, OrmLiteConStr> func);
    }
}
