using static FclEx.EfCore.Extensions.ApplyKeyTestContext;

namespace FclEx.EfCore.Extensions.DbContextExtensions;

public class ApplyKeyToTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ApplyKeyTo_CopiesCompositeKey_AndReturnsContext(bool transformValues)
    {
        using var context = new ApplyKeyTestContext();
        var source = new CompositeItem { TenantId = 5, Code = "source", Name = "source" };
        var target = new CompositeItem { TenantId = 9, Code = "target", Name = "target" };
        var visited = new List<string>();

        var result = context.ApplyKeyTo(source, target, transformValues ? (property, value) =>
        {
            visited.Add(property.Name);
            return property.Name == nameof(CompositeItem.TenantId) ? (int)value! + 1 : (string)value! + "!";
        } : null);

        Assert.Same(context, result);
        Assert.Equal(transformValues ? 6 : 5, target.TenantId);
        Assert.Equal(transformValues ? "source!" : "source", target.Code);
        Assert.Equal("target", target.Name);
        Assert.Equal(5, source.TenantId);
        Assert.Equal("source", source.Code);
        Assert.Equal(transformValues ? 2 : 0, visited.Count);
        Assert.Empty(context.ChangeTracker.Entries());
        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
    }

    [Fact]
    public void ApplyKeyTo_CopiesOnlyPrimaryKey()
    {
        using var context = new ApplyKeyTestContext();
        var target = new Item { Id = 1, Code = "target", Name = "target" };
        context.ApplyKeyTo(new Item { Id = 42, Code = "source", Name = "source" }, target);
        Assert.Equal(42, target.Id);
        Assert.Equal("target", target.Code);
        Assert.Equal("target", target.Name);
    }

    [Fact]
    public void ApplyKeyTo_RejectsUnmappedType()
    {
        using var context = new ApplyKeyTestContext();
        var error = Assert.Throws<InvalidOperationException>(() => context.ApplyKeyTo(new object(), new object()));
        Assert.Contains("is not an entity type", error.Message);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public void ApplyKeyTo_RejectsKeylessType()
    {
        using var context = new ApplyKeyTestContext();
        var error = Assert.Throws<InvalidOperationException>(() => context.ApplyKeyTo(new KeylessItem(), new KeylessItem()));
        Assert.Contains("does not have a primary key", error.Message);
    }

    [Fact]
    public void ApplyKeyTo_RejectsShadowKey()
    {
        using var context = new ApplyKeyTestContext();
        var error = Assert.Throws<InvalidOperationException>(() => context.ApplyKeyTo(new ShadowItem(), new ShadowItem()));
        Assert.Contains("shadow property", error.Message);
        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
    }

    [Fact]
    public void ApplyKeyTo_ReadsClrValue_InsteadOfTemporaryEntryValue()
    {
        using var context = new ApplyKeyTestContext();
        var source = new Item();
        var entry = context.Add(source);
        var temporaryValue = entry.Property(entity => entity.Id).CurrentValue;
        Assert.True(entry.Property(entity => entity.Id).IsTemporary);
        Assert.NotEqual(0, temporaryValue);
        Assert.Equal(0, source.Id);
        var target = new Item { Id = 7 };

        context.ApplyKeyTo(source, target);

        Assert.Equal(0, target.Id);
        Assert.Equal(temporaryValue, entry.Property(entity => entity.Id).CurrentValue);
        Assert.Same(source, Assert.Single(context.ChangeTracker.Entries()).Entity);
    }
}
