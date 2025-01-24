namespace FclEx.EfCore;

public class QueryableHelperTests
{
    private class TestEntity : IHasId<int>
    {
        public int Id { get; set; }
    }

    private class TestEntityWithStringId : IHasId<string>
    {
        public string Id { get; set; } = string.Empty;
    }

    [Fact]
    public void BuildIdFilter_GeneratesCorrectExpression_ForIntId()
    {
        const int id = 42;
        var filter = QueryableHelper.BuildIdFilter<TestEntity, int>(id);

        var testEntity = new TestEntity { Id = id };
        Assert.True(filter.Compile()(testEntity));

        testEntity.Id = 100;
        Assert.False(filter.Compile()(testEntity));
    }

    [Fact]
    public void BuildIdFilter_GeneratesCorrectExpression_ForStringId()
    {
        const string id = "abc";
        var filter = QueryableHelper.BuildIdFilter<TestEntityWithStringId, string>(id);

        var testEntity = new TestEntityWithStringId { Id = id };
        Assert.True(filter.Compile()(testEntity));

        testEntity.Id = "xyz";
        Assert.False(filter.Compile()(testEntity));
    }
}
