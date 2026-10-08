using Microsoft.EntityFrameworkCore.Metadata;

namespace FclEx.EfCore;

internal static class TruncateTestSessionExtensions
{
    public static TestDbContext CreateDbContext(this TruncateTestSession session, EfCoreFixture fixture, string? originalSchema)
    {
        using var reference = fixture.CreateDbContext(session.Driver, originalSchema);
        var builder = new ModelBuilder(ConventionSet.CreateConventionSet(reference));
        reference.ConfigureModel(builder);
        foreach (var entity in builder.Model.GetEntityTypes().ToArray())
            builder.Entity(entity.ClrType).ToTable(session.GetTableName(entity.ClrType), session.Schema);
        var options = new DbContextOptionsBuilder<TestDbContext>();
        options.UseModel(builder.FinalizeModel());
        var context = new TestDbContext(session.Driver, reference.ConnectionString, session.Schema, options.Options);
        context.Database.SetDbConnection(session.Connection);
        return context;
    }

    public static TestDbContext CreateSharedModelContext(this TestDbContext reference)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseModel(reference.Model);
        var context = new TestDbContext(reference.DbDriver, reference.ConnectionString, reference.Schema, options.Options);
        context.Database.SetDbConnection(reference.Database.GetDbConnection());
        return context;
    }
}
