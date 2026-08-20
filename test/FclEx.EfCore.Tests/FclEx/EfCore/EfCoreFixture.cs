namespace FclEx.EfCore;

public class EfCoreFixture : DapperTestsFixture
{
    internal static Assembly Assembly => typeof(EfCoreFixture).Assembly;
    internal static readonly string?[] Schemas = SchemaNames.Select(m => WithAssemblyInfo(m, Assembly)).ToArray();

    public override string?[] CurrentSchemas => Schemas;

    public TestDbContext CreateDbContext(DbDriver dbDriver, string? schema = null, bool isUser = false)
    {
        var con = ConnectionStrings.Get(dbDriver, isUser).Build();
        return new(dbDriver, con, schema);
    }
}