using Microsoft.EntityFrameworkCore.Metadata;

namespace FclEx.EfCore;

internal static class TruncateTestSessionExtensions
{
    public static TruncateDbContext CreateDbContext(this TruncateTestSession session, EfCoreFixture fixture, string? originalSchema)
    {
        using var reference = new TruncateDbContext(session.Driver,
            fixture.ResolveTarget(session.Driver, originalSchema).BuildConnectionString(), originalSchema);
        var builder = new ModelBuilder(ConventionSet.CreateConventionSet(reference));
        reference.ConfigureModel(builder);
        foreach (var entity in builder.Model.GetEntityTypes().ToArray())
            builder.Entity(entity.ClrType).ToTable(session.GetTableName(entity.ClrType), session.Schema);
        var options = new DbContextOptionsBuilder<TestDbContext>();
        options.UseModel(builder.FinalizeModel());
        var context = new TruncateDbContext(session.Driver, reference.ConnectionString, session.Schema, options.Options);
        context.Database.SetDbConnection(session.Connection);
        return context;
    }

    public static TruncateDbContext CreateSharedModelContext(this TruncateDbContext reference)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseModel(reference.Model);
        var context = new TruncateDbContext(reference.DbDriver, reference.ConnectionString, reference.Schema, options.Options);
        context.Database.SetDbConnection(reference.Database.GetDbConnection());
        return context;
    }
}
