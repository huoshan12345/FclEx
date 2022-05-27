
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace FclEx.Abp.OrmLite
{
    public class OrmLiteConStr
    {
        public OrmLiteConStr(string name, string str, IOrmLiteDialectProvider provider)
        {
            Name = Check.NotNull(name);
            Str = Check.NotNull(str);
            Provider = Check.NotNull(provider);
            ConFac = new OrmLiteConnectionFactory(str, provider, false);
        }

        public OrmLiteConStr(string str, IOrmLiteDialectProvider provider)
            : this(str, str, provider)
        {
        }

        public string Name { get; }
        public string Str { get; }
        public IOrmLiteDialectProvider Provider { get; }
        public IDbConnectionFactory ConFac { get; }
    }
}
