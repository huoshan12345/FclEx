using FclEx.Dapper;

namespace FclEx.EfCore;

public class EfCoreFixture : DapperTestsFixture
{
    public TestDbContext CreateDbContext(DbDriver dbDriver, string? schema = null, bool isUser = false)
    {
        var con = ConnectionStrings.Get(dbDriver, isUser).Build();
        return new(dbDriver, con, schema);
    }
}