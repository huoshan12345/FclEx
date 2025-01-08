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

        var appliedFilter = entityType.GetQueryFilter();
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

        Assert.Null(entityType.GetQueryFilter());
    }

    [Fact]
    public void HasQueryFilter_ShouldHandleNullFilter()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());
        var entityType = modelBuilder.Model.AddEntityType(typeof(TestEntity));

        var result = modelBuilder.HasQueryFilter<TestEntity>(entityType, null);

        Assert.Null(entityType.GetQueryFilter());
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
}