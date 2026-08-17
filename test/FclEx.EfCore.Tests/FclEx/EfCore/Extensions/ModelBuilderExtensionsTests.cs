namespace FclEx.EfCore.Extensions;

public class ModelBuilderExtensionsTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
    }

    private class AnotherEntity
    {
        public int Id { get; set; }
    }

    [Fact]
    public void HasQueryFilter_ShouldApplyFilter_WhenTypeMatches()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());
        var entityType = modelBuilder.Model.AddEntityType(typeof(TestEntity));
        Expression<Func<TestEntity, bool>> filter = e => !e.IsDeleted;
        var result = modelBuilder.HasQueryFilter(entityType, filter);
        Assert.Equal(modelBuilder, result);

#if NET10_0_OR_GREATER
        var appliedFilter = entityType.GetDeclaredQueryFilters().FirstOrDefault()?.Expression;
#else
        var appliedFilter = entityType.GetQueryFilter();
#endif
        Assert.NotNull(appliedFilter);

        var queryFilter = appliedFilter.ToString();
        Assert.Contains("Param_0 => Not(Param_0.IsDeleted)", queryFilter);
    }

    [Fact]
    public void HasQueryFilter_ShouldNotApplyFilter_WhenTypeDoesNotMatch()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());
        var entityType = modelBuilder.Model.AddEntityType(typeof(AnotherEntity));
        Expression<Func<TestEntity, bool>> filter = e => !e.IsDeleted;
        var result = modelBuilder.HasQueryFilter(entityType, filter);
        Assert.Equal(modelBuilder, result);

#if NET10_0_OR_GREATER
        var appliedFilter = entityType.GetDeclaredQueryFilters().FirstOrDefault()?.Expression;
#else
        var appliedFilter = entityType.GetQueryFilter();
#endif
        Assert.Null(appliedFilter);
    }

    [Fact]
    public void HasQueryFilter_ShouldHandleNullFilter()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());
        var entityType = modelBuilder.Model.AddEntityType(typeof(TestEntity));
        var result = modelBuilder.HasQueryFilter<TestEntity>(entityType, null);
        Assert.Equal(modelBuilder, result);

#if NET10_0_OR_GREATER
        var appliedFilter = entityType.GetDeclaredQueryFilters().FirstOrDefault()?.Expression;
#else
        var appliedFilter = entityType.GetQueryFilter();
#endif
        Assert.Null(appliedFilter);
    }

    [Fact]
    public void HasQueryFilter_ShouldReturnSameModelBuilder()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());
        var entityType = modelBuilder.Model.AddEntityType(typeof(TestEntity));
        Expression<Func<TestEntity, bool>> filter = e => !e.IsDeleted;
        var result = modelBuilder.HasQueryFilter(entityType, filter);
        Assert.Equal(modelBuilder, result);
    }

    [Fact]
    public void ExcludeFromMigrations_ShouldConfigureEntityMapping()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());

        var builder = modelBuilder.ExcludeFromMigrations<TestEntity>("tests", "archive");

        Assert.Equal("tests", builder.Metadata.GetTableName());
        Assert.Equal("archive", builder.Metadata.GetSchema());
        Assert.True(builder.Metadata.IsTableExcludedFromMigrations());
    }

#if NET10_0_OR_GREATER
    [Fact]
    public void HasQueryFilter_ShouldApplyNamedFilter()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());
        var entityType = modelBuilder.Model.AddEntityType(typeof(TestEntity));
        Expression<Func<TestEntity, bool>> filter = entity => !entity.IsDeleted;

        var result = modelBuilder.HasQueryFilter(entityType, "soft-delete", filter);

        Assert.Same(modelBuilder, result);
        var applied = Assert.Single(entityType.GetDeclaredQueryFilters());
        var expression = Assert.IsAssignableFrom<LambdaExpression>(applied.Expression);
        var compiled = (Func<TestEntity, bool>)expression.Compile();
        Assert.True(compiled(new TestEntity()));
        Assert.False(compiled(new TestEntity { IsDeleted = true }));
    }
#endif
}
