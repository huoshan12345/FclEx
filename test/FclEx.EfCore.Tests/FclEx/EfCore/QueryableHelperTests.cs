namespace FclEx.EfCore;

public class QueryableHelperTests
{
    private class TestEntity : IHasId<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestEntityWithStringId : IHasId<string>
    {
        public string Id { get; set; } = string.Empty;
    }

    private class TestEntityWithNullableId : IHasId<int?>
    {
        public int? Id { get; set; }
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
    public void BuildIdFilter_GeneratesCorrectExpression_ForNullNullableId()
    {
        var filter = QueryableHelper.BuildIdFilter<TestEntityWithNullableId, int?>(null).Compile();

        Assert.True(filter(new TestEntityWithNullableId()));
        Assert.False(filter(new TestEntityWithNullableId { Id = 1 }));
    }

    [Theory]
    [InlineData(false, @"\")]
    [InlineData(true, @"\\")]
    public void BuildLike_UsesConfiguredEscapeCharacter(bool escapeEscapeCharacter, string expectedEscapeCharacter)
    {
        var filter = QueryableHelper.BuildLike<TestEntity>(
            entity => entity.Name,
            "%name%",
            suppressValueConverter: false,
            escapeEscapeCharacter);

        var call = Assert.IsAssignableFrom<MethodCallExpression>(filter.Body);
        Assert.Equal(nameof(DbFunctionsExtensions.Like), call.Method.Name);
        var escapeCharacter = Assert.IsType<ConstantExpression>(call.Arguments[3]);
        Assert.Equal(expectedEscapeCharacter, escapeCharacter.Value);
    }

    [Fact]
    public void BuildContainsAny_GeneratesCorrectExpression_ForStringId()
    {
        var filter = QueryableHelper.BuildContainsAny<TestEntity>(m => m.Name, ["Tom", "Jerry", "Linda"]);

        Assert.NotNull(filter);
        Assert.Contains(nameof(DbFunctionsExtensions.Like), filter.ToString());
        Assert.Contains(nameof(ExpressionType.OrElse), filter.ToString());
        Assert.DoesNotContain("%Tom%", filter.ToString());
    }

    [Fact]
    public void BuildContainsAny_ReturnsNull_WhenKeywordsAreEmpty()
    {
        var filter = QueryableHelper.BuildContainsAny<TestEntity>(entity => entity.Name, []);

        Assert.Null(filter);
    }

}
