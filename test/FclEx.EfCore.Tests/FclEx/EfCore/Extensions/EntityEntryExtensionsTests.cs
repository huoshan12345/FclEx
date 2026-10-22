using static FclEx.EfCore.Extensions.ApplyKeyTestContext;

namespace FclEx.EfCore.Extensions;

public class EntityEntryExtensionsTests
{
    [Fact]
    public void ApplyKeyTo_CopiesFromEntryToTarget_AndReturnsSourceEntry()
    {
        using var context = new ApplyKeyTestContext();
        var source = new Item { Id = 42, Code = "source", Name = "source" };
        var entry = context.Attach(source);
        var target = new Item { Id = 7, Code = "target", Name = "target" };

        var result = entry.ApplyKeyTo(target);

        Assert.Same(entry, result);
        Assert.Equal(42, target.Id);
        Assert.Equal("target", target.Code);
        Assert.Equal("target", target.Name);
        Assert.Equal(42, source.Id);
        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.Same(source, Assert.Single(context.ChangeTracker.Entries()).Entity);
        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
    }

    [Fact]
    public void ApplyKeyTo_TransformsEachCompositeKeyComponent()
    {
        using var context = new ApplyKeyTestContext();
        var source = new CompositeItem { TenantId = 5, Code = "source" };
        var entry = context.Entry(source);
        var target = new CompositeItem { Name = "target" };
        var visited = new List<string>();

        entry.ApplyKeyTo(target, (property, value) =>
        {
            Assert.Same(entry.Metadata.FindProperty(property.Name), property);
            visited.Add(property.Name);
            return property.Name == nameof(CompositeItem.TenantId) ? (int)value! + 1 : (string)value! + "!";
        });

        Assert.Equal(6, target.TenantId);
        Assert.Equal("source!", target.Code);
        Assert.Equal("target", target.Name);
        Assert.Equal(entry.Metadata.FindPrimaryKey()!.Properties.Select(property => property.Name), visited);
        Assert.Equal(EntityState.Detached, entry.State);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public void ApplyKeyTo_ReadsTemporaryCurrentValue_WithoutCopyingTrackingFlags()
    {
        using var context = new ApplyKeyTestContext();
        var source = new Item();
        var entry = context.Add(source);
        var temporaryValue = entry.Property(entity => entity.Id).CurrentValue;
        Assert.True(entry.Property(entity => entity.Id).IsTemporary);
        Assert.NotEqual(0, temporaryValue);
        var target = new Item { Id = 7 };

        entry.ApplyKeyTo(target);

        Assert.Equal(0, source.Id);
        Assert.Equal(temporaryValue, target.Id);
        Assert.Equal(temporaryValue, entry.Property(entity => entity.Id).CurrentValue);
        Assert.True(entry.Property(entity => entity.Id).IsTemporary);
        Assert.Equal(EntityState.Detached, context.Entry(target).State);
        Assert.False(context.Entry(target).Property(entity => entity.Id).IsTemporary);
    }

    [Fact]
    public void ApplyKeyTo_CopiesFieldOnlyKey()
    {
        using var context = new ApplyKeyTestContext();
        var entry = context.Entry(new FieldItem { Id = 42 });
        var target = new FieldItem { Id = 7, Name = "target" };
        entry.ApplyKeyTo(target);
        Assert.Equal(42, target.Id);
        Assert.Equal("target", target.Name);
    }

    [Fact]
    public void ApplyKeyTo_ReadsShadowKey_ButDoesNotWriteTargetClrMembers()
    {
        using var context = new ApplyKeyTestContext();
        var entry = context.Entry(new ShadowItem { Name = "source" });
        entry.Property("Id").CurrentValue = 42;
        var target = new ShadowItem { Name = "target" };
        var calls = 0;

        entry.ApplyKeyTo(target, (property, value) =>
        {
            Assert.True(property.IsShadowProperty());
            Assert.Equal(42, value);
            calls++;
            return 99;
        });

        Assert.Equal(1, calls);
        Assert.Equal("target", target.Name);
        Assert.Equal(42, entry.Property("Id").CurrentValue);
        Assert.Equal(0, context.Entry(target).Property("Id").CurrentValue);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public void ApplyKeyTo_PropagatesTransformFailure_WithoutRollingBackEarlierComponents()
    {
        using var context = new ApplyKeyTestContext();
        var entry = context.Entry(new CompositeItem { TenantId = 42, Code = "source" });
        var target = new CompositeItem { TenantId = 7, Code = "target" };
        var expected = new InvalidOperationException("transform failed");

        var actual = Assert.Throws<InvalidOperationException>(() => entry.ApplyKeyTo(target, (property, value) =>
            property.Name == nameof(CompositeItem.Code) ? throw expected : value));

        Assert.Same(expected, actual);
        Assert.Equal(42, target.TenantId);
        Assert.Equal("target", target.Code);
    }

    [Fact]
    public void ApplyKeyToDefault_ResetsCompositeKey_WithoutChangingSourceOrOtherMembers()
    {
        using var context = new ApplyKeyTestContext();
        var source = new CompositeItem { TenantId = 42, Code = "source" };
        var entry = context.Attach(source);
        var target = new CompositeItem { TenantId = 7, Code = "target", Name = "target" };

        var result = entry.ApplyKeyToDefault(target);

        Assert.Same(entry, result);
        Assert.Equal(0, target.TenantId);
        Assert.Null(target.Code);
        Assert.Equal("target", target.Name);
        Assert.Equal(42, source.TenantId);
        Assert.Equal("source", source.Code);
        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.Same(source, Assert.Single(context.ChangeTracker.Entries()).Entity);
    }
}
