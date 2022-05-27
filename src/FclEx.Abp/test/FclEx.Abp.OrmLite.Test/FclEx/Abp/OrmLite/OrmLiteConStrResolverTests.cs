using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Abp.OrmLite
{
    public class OrmLiteConStrResolverTests : AbpOrmLiteTest
    {
        public OrmLiteConStrResolverTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GetConStrs_Test()
        {
            var resolver = ServiceProvider.GetRequiredService<IOrmLiteConStrResolver>();
            Assert.IsType<OrmLiteConStrResolver>(resolver);

            var cons = resolver.GetConStrs();
            Assert.Single(cons);

            var con = cons.First();
            Assert.Equal(GlobalConstants.MainConStrKey, con.Name);
            Assert.IsType<EmptyOrmLiteDialectProvider>(con.Provider);
        }
    }
}
