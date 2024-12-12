namespace FclEx.EfCore.Extensions;

public class QueryableExtensionsTests : DbContextTests
{
    public static readonly IEnumerable<object?[]> ContainsAnyTestCases = DbTestCases
        .SelectMany([true, false])
        .Select(x => x.Left.Append(x.Right).ToArray());

    [Theory]
    [MemberData(nameof(ContainsAnyTestCases))]
    public async Task ContainsAny_Test(DbProviderType dbProviderType, bool containsPercentSign)
    {
        await using var context = GlobalDbContext.Create(dbProviderType);
        await context.EntityWithAutoKeys.ExecuteDeleteAsync();

        var list = Enumerable.Range(1, 9)
            .Select(CreateName)
            .Select(m => new EntityWithAutoKey
            {
                Name = m,
                Value = 1,
            });
        context.EntityWithAutoKeys.AddRange(list);
        await context.SaveChangesAsync();

        var keywords = new[] { CreateKeyword(4), CreateKeyword(6) };

        var result = await context.EntityWithAutoKeys
            .ContainsAny(m => m.Name, keywords)
            .ToListAsync();

        Assert.Equal(2, result.Count);
        Assert.True(keywords.All(m => result.Any(x => x.Name!.Contains(m))));

        string CreateKeyword(int number)
        {
            return StringBuilderHelper.Build(m =>
            {
                m.Append(number);
                if (containsPercentSign)
                {
                    m.Append('%');
                }
                m.Append(number);
            });
        }

        string CreateName(int number)
        {
            return StringBuilderHelper.Build(m =>
            {
                m.Append("prefix_");
                m.Append(CreateKeyword(number));
                m.Append("_postfix");
            });
        }
    }
}