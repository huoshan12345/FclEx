using static FclEx.EfCore.Extensions.ApplyKeyTestContext;

namespace FclEx.EfCore.Extensions;

public class EntityTypeExtensionsTests
{
    [Fact]
    public void ApplyKeyTo_CopiesOnlyPrimaryKey_WithoutTrackingOrOpeningConnection()
    {
        using var context = new ApplyKeyTestContext();
        var source = new Item { Id = 42, Code = "source", Name = "source" };
        var target = new Item { Id = 7, Code = "target", Name = "target" };

        context.Model.FindEntityType(typeof(Item))!.ApplyKeyTo(source, target);

        Assert.Equal(42, target.Id);
        Assert.Equal("target", target.Code);
        Assert.Equal("target", target.Name);
        Assert.Equal(42, source.Id);
        Assert.Empty(context.ChangeTracker.Entries());
        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ApplyKeyTo_CopiesCompositeKey_WithOptionalTransformation(bool transformValues)
    {
        using var context = new ApplyKeyTestContext();
        var entityType = context.Model.FindEntityType(typeof(CompositeItem))!;
        var source = new CompositeItem { TenantId = 4, Code = "source", Name = "source" };
        var target = new CompositeItem { TenantId = 1, Code = "target", Name = "target" };
        var visited = new List<string>();

        entityType.ApplyKeyTo(source, target, transformValues ? (property, value) =>
        {
            Assert.Same(entityType.FindProperty(property.Name), property);
            visited.Add(property.Name);
            return property.Name == nameof(CompositeItem.TenantId) ? (int)value! + 10 : (string)value! + "!";
        } : null);

        Assert.Equal(transformValues ? 14 : 4, target.TenantId);
        Assert.Equal(transformValues ? "source!" : "source", target.Code);
        Assert.Equal("target", target.Name);
        Assert.Equal(4, source.TenantId);
        Assert.Equal("source", source.Code);
        if (transformValues)
            Assert.Equal(entityType.FindPrimaryKey()!.Properties.Select(property => property.Name), visited);
    }

    [Fact]
    public void ApplyKeyTo_CopiesFieldOnlyKey()
    {
        using var context = new ApplyKeyTestContext();
        var target = new FieldItem { Id = 1, Name = "target" };
        context.Model.FindEntityType(typeof(FieldItem))!.ApplyKeyTo(new FieldItem { Id = 17 }, target);
        Assert.Equal(17, target.Id);
        Assert.Equal("target", target.Name);
    }

    [Fact]
    public void ApplyKeyTo_RejectsMismatchedClrType_BeforeTransforming()
    {
        using var context = new ApplyKeyTestContext();
        var called = false;
        var target = new FieldItem { Id = 7 };
        var error = Assert.Throws<InvalidOperationException>(() =>
            context.Model.FindEntityType(typeof(Item))!.ApplyKeyTo(new FieldItem { Id = 42 }, target, (_, value) =>
            {
                called = true;
                return value;
            }));
        Assert.Contains("does not match", error.Message);
        Assert.False(called);
        Assert.Equal(7, target.Id);
    }

    [Fact]
    public void ApplyKeyTo_RejectsKeylessType()
    {
        using var context = new ApplyKeyTestContext();
        var error = Assert.Throws<InvalidOperationException>(() =>
            context.Model.FindEntityType(typeof(KeylessItem))!.ApplyKeyTo(new KeylessItem(), new KeylessItem()));
        Assert.Contains("does not have a primary key", error.Message);
    }

    [Fact]
    public void ApplyKeyTo_RejectsShadowKey_BeforeTransforming()
    {
        using var context = new ApplyKeyTestContext();
        var called = false;
        var error = Assert.Throws<InvalidOperationException>(() =>
            context.Model.FindEntityType(typeof(ShadowItem))!.ApplyKeyTo(new ShadowItem(), new ShadowItem(), (_, value) =>
            {
                called = true;
                return value;
            }));
        Assert.Contains("shadow property", error.Message);
        Assert.False(called);
    }

    [Fact]
    public void ApplyKeyTo_PropagatesTransformFailure_WithoutRollingBackEarlierComponents()
    {
        using var context = new ApplyKeyTestContext();
        var source = new CompositeItem { TenantId = 42, Code = "source" };
        var target = new CompositeItem { TenantId = 7, Code = "target" };
        var expected = new InvalidOperationException("transform failed");

        var actual = Assert.Throws<InvalidOperationException>(() =>
            context.Model.FindEntityType(typeof(CompositeItem))!.ApplyKeyTo(source, target, (property, value) =>
                property.Name == nameof(CompositeItem.Code) ? throw expected : value));

        Assert.Same(expected, actual);
        Assert.Equal(42, target.TenantId);
        Assert.Equal("target", target.Code);
    }
}
