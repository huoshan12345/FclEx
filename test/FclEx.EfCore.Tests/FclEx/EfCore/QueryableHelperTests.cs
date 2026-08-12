namespace FclEx.EfCore;

public class QueryableHelperTests
{
    private class TestEntity : IHasId<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? Order { get; set; }
    }

    private class TestEntityWithStringId : IHasId<string>
    {
        public string Id { get; set; } = string.Empty;
    }

    [Theory]
    [InlineData("plain", "%plain%")]
    [InlineData("a%b", @"%a\%b%")]
    [InlineData("a_b", @"%a\_b%")]
    [InlineData(@"a\b", @"%a\\b%")]
    [InlineData("a[b", @"%a\[b%")]
    [InlineData(@"a\%_[b", @"%a\\\%\_\[b%")]
    public void GetContainsPattern_ShouldEscapeLikeMetacharacters(string value, string expected)
    {
        Assert.Equal(expected, QueryableHelper.GetContainsPattern(value));
    }

    [Fact]
    public void GetContainsPattern_WhenEscapeCharacterRequiresSqlEscaping_ShouldDoubleEscapeCharacter()
    {
        Assert.Equal(@"%a\\\\b%", QueryableHelper.GetContainsPattern(@"a\b", escapeEscapeCharacter: true));
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

    [Fact]
    public void BuildContainsAny_GeneratesCorrectExpression_ForStringId()
    {
        var filter = QueryableHelper.BuildContainsAny<TestEntity>(m => m.Name, ["Tom", "Jerry", "Linda"]);

        Assert.NotNull(filter);
        Assert.Equal("m => ((value(Microsoft.EntityFrameworkCore.DbFunctions).Like(m.Name, \"%Tom%\", \"\\\") " +
                     "OrElse value(Microsoft.EntityFrameworkCore.DbFunctions).Like(m.Name, \"%Jerry%\", \"\\\")) " +
                     "OrElse value(Microsoft.EntityFrameworkCore.DbFunctions).Like(m.Name, \"%Linda%\", \"\\\"))", filter.ToString());
    }

    [Theory]
    [InlineData(42, "match", 7, true)]
    [InlineData(42, "other", 7, false)]
    [InlineData(42, "match", 8, false)]
    [InlineData(42, "match", null, false)]
    public void BuildFilter_CombinesPropertiesWithinCompositeIndexWithAnd(int id, string name, int? order, bool expected)
    {
        using var context = new CompositeIndexContext();
        var entity = new TestEntity { Id = 42, Name = "match", Order = 7 };
        var index = context.Model.FindEntityType(typeof(TestEntity))!.GetIndexes().Single();

        var filter = QueryableHelper.BuildFilter([index], entity).Compile();

        Assert.Equal(expected, filter(new TestEntity { Id = id, Name = name, Order = order }));
    }

    [Fact]
    public void BuildFilter_SupportsNullNullableIndexValue()
    {
        using var context = new CompositeIndexContext();
        var entity = new TestEntity { Id = 42, Name = "match", Order = null };
        var index = context.Model.FindEntityType(typeof(TestEntity))!.GetIndexes().Single();

        var filter = QueryableHelper.BuildFilter([index], entity).Compile();

        Assert.True(filter(new TestEntity { Id = 42, Name = "match", Order = null }));
        Assert.False(filter(new TestEntity { Id = 42, Name = "match", Order = 7 }));
    }

    private sealed class CompositeIndexContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=:memory:");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>().HasIndex(e => new { e.Id, e.Name, e.Order });
        }
    }
}
