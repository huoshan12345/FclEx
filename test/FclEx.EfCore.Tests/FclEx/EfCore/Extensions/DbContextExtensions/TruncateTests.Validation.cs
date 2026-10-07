using Microsoft.EntityFrameworkCore.Metadata;

namespace FclEx.EfCore.Extensions.DbContextExtensions;

public partial class TruncateTests
{
    public static TheoryData<string> UnsupportedMappingCases => new()
    {
        "tph", "tpt", "tpc", "split", "owned", "table-sharing", "view",
    };

    [Theory]
    [MemberData(nameof(UnsupportedMappingCases))]
    public async Task TruncateAsync_RejectsUnsupportedMappings(string shape)
    {
        await using var context = CreateMappingContext(Fixture, shape);
        var entityType = context.Model.FindEntityType(typeof(MappingEntity))!;
        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync<MappingEntity>());
        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync<MappingEntity>(true, false));
        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync(typeof(MappingEntity), true, false));
        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync(entityType));
        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync(entityType, true, false));
        if (shape is "tph" or "tpt" or "tpc")
            await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync<DerivedMappingEntity>());
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task TruncateAsync_RejectsUnknownEntity(DbDriver driver)
    {
        Assert.SkipMySql(driver);
        await using var context = Fixture.CreateDbContext(driver);
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.TruncateAsync<string>());
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.TruncateAsync<string>(true, false));
    }

    [Fact]
    public void TruncateDialect_RejectsUnknownConnectionTypes()
    {
        var exception = Assert.Throws<NotSupportedException>(() =>
            FclEx.EfCore.DbContextExtensions.GetTruncateDialect(typeof(DbConnection)));
        Assert.Contains(typeof(DbConnection).FullName!, exception.Message);
    }

    // Only mapping-validation tests need alternative models. They neither create tables nor execute SQL.
    internal static DbContext CreateMappingContext(EfCoreFixture fixture, string shape)
    {
        using var reference = fixture.CreateDbContext(DbDriver.SqlServer);
        return CreateModelContext(reference, builder =>
        {
            var entity = builder.Entity<MappingEntity>();
            entity.ToTable(nameof(EntityWithAutoKey));
            switch (shape)
            {
                case "tph":
                    builder.Entity<DerivedMappingEntity>();
                    break;
                case "tpt":
                    entity.UseTptMappingStrategy();
                    builder.Entity<DerivedMappingEntity>().ToTable(nameof(EntityWithIdAndIndex));
                    break;
                case "tpc":
                    entity.UseTpcMappingStrategy();
                    builder.Entity<DerivedMappingEntity>().ToTable(nameof(EntityWithIdAndIndex));
                    break;
                case "split":
                    entity.SplitToTable(nameof(EntityWithIdAndIndex), table => table.Property(row => row.Name));
                    break;
                case "owned":
                    entity.OwnsOne(row => row.Details);
                    return;
                case "view":
                    entity.ToTable((string?)null);
                    entity.ToView(nameof(EntityWithAutoKey));
                    break;
                case "table-sharing":
                    builder.Entity<OtherMappingEntity>().ToTable(nameof(EntityWithAutoKey));
                    entity.HasOne<OtherMappingEntity>().WithOne().HasForeignKey<OtherMappingEntity>(row => row.Id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shape), shape, null);
            }
            entity.Ignore(row => row.Details);
        });
    }

    private static DbContext CreateModelContext(TestDbContext reference, Action<ModelBuilder> configureModel)
    {
        var builder = new ModelBuilder(ConventionSet.CreateConventionSet(reference));
        // The convention set initially discovers TestDbContext's DbSets. These alternative
        // models must contain only the mappings explicitly configured by this test.
        foreach (var entity in builder.Model.GetEntityTypes().ToArray())
            builder.Ignore(entity.ClrType);
        configureModel(builder);
        var model = builder.FinalizeModel();
        // Reuse the real provider options and connection string without an extra context subclass or cache key.
        var options = new DbContextOptionsBuilder();
        foreach (var extension in reference.GetService<IDbContextOptions>().Extensions)
            ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(extension);
        options.UseModel(model);
        var context = new DbContext(options.Options);
        // Temporary tables must share the open session. Validation-only models retain
        // their own connection so disposing their short-lived reference does not dispose it.
        if (reference.Database.GetDbConnection().State == ConnectionState.Open)
            context.Database.SetDbConnection(reference.Database.GetDbConnection());
        return context;
    }

    internal class MappingEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Value { get; set; }
        public MappingDetails? Details { get; set; }
    }

    private sealed class DerivedMappingEntity : MappingEntity;
    private sealed class OtherMappingEntity
    {
        public int Id { get; set; }
    }

    internal sealed class MappingDetails
    {
        public string? Value { get; set; }
    }
}
