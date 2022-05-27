using System.Collections.Generic;

namespace FclEx.Abp.OrmLite
{
    public class AbpOrmLiteOptions
    {
        public List<OrmLiteConStr> ConStrs { get; } = new List<OrmLiteConStr>();
    }
}
