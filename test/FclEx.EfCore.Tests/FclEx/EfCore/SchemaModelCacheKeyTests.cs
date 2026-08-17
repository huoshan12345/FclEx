namespace FclEx.EfCore;

public class SchemaModelCacheKeyTests
{
    [Fact]
    public void Equality_IncludesContextTypeSchemaAndDesignTime()
    {
        using var firstContext = new SchemaDbContext(new DbContextOptionsBuilder().Options, "tenant_a");
        using var secondContext = new SchemaDbContext(new DbContextOptionsBuilder().Options, "tenant_a");

        var key = new SchemaModelCacheKey(firstContext, "tenant_a", designTime: false);
        var equivalent = new SchemaModelCacheKey(secondContext, "tenant_a", designTime: false);
        var otherSchema = new SchemaModelCacheKey(secondContext, "tenant_b", designTime: false);
        var designTime = new SchemaModelCacheKey(secondContext, "tenant_a", designTime: true);

        Assert.Equal(key, equivalent);
        Assert.Equal(key.GetHashCode(), equivalent.GetHashCode());
        Assert.NotEqual(key, otherSchema);
        Assert.NotEqual(key, designTime);
    }

    [Fact]
    public void Factory_ReadsSchemaAndDesignTimeFromContext()
    {
        using var context = new SchemaDbContext(new DbContextOptionsBuilder().Options, "tenant");
        var factory = new SchemaModelCacheKeyFactory();

        var key = Assert.IsType<SchemaModelCacheKey>(factory.Create(context, designTime: true));

        Assert.Equal("tenant", key.Schema);
        Assert.True(key.DesignTime);
    }

    [Fact]
    public void Factory_UsesNullSchemaForOrdinaryContext()
    {
        using var context = new DbContext(new DbContextOptionsBuilder().Options);
        var factory = new SchemaModelCacheKeyFactory();

        var key = Assert.IsType<SchemaModelCacheKey>(factory.Create(context, designTime: false));

        Assert.Null(key.Schema);
        Assert.False(key.DesignTime);
    }
}
