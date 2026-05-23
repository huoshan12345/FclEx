namespace FclEx.Utils;

public class ExpressionReplacerTests
{
    public record Person(string? Name, int Age);

    [Fact]
    public void ReplaceParameters_Test()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        Expression<Func<List<int>, int, bool>> filter = (l, x) => (l != null && x % 2 == 0);
        var list = Enumerable.Range(1, 100).ToList();
        var items = list.Where(ReplaceParameter(list, filter).Compile()).ToList();
        Assert.Equal(list.Where(x => x % 2 == 0), items);
    }

    [Fact]
    public void ReplaceParameters_WhitNullParameter_Test()
    {
        Expression<Func<List<Person>, Person, bool>> filter = (l, x) => (l.Count - x.Age > 0 && x.Name == null);
        var list = Enumerable.Range(1, 100).Select(m => new Person(m % 3 == 0 ? null : m.ToString(), m)).ToList();
        var items = list.Where(ReplaceParameter(list, filter).Compile()).ToList();
        Assert.Equal(list.Where(x => (list.Count - x.Age > 0 && x.Name == null)), items);
    }

    [Fact]
    public void ReplaceParameters_Multi_Times_Test()
    {
        Expression<Func<List<int>, int, bool>> filter = (l, x) => x % 2 == 0;

        var list = Enumerable.Range(1, 100).ToList();
        List<int> p = list;
        for (var i = 0; i < 10; ++i)
        {
            p = list.Where(ReplaceParameter(p, filter).Compile()).ToList();
        }
        Assert.Equal(list.Where(x => x % 2 == 0), p);
    }

    [Fact]
    public async Task ReplaceParameters_Multi_Tasks_Test()
    {
        Expression<Func<List<int>, int, bool>> filter = (l, x) => x % 2 == 0;

        var list = Enumerable.Range(1, 10).ToList();
        var tasks = Enumerable.Range(1, 100).Select(m => (Func<Task<List<int>>>)(() => Task.Run(() => list.Where(ReplaceParameter(list, filter).Compile()).ToList())));
        var results = await tasks.Select(m => m()).WhenAll();
        var expected = list.Where(x => x % 2 == 0).ToList();

        Assert.Equal(100, results.Length);
        foreach (var result in results)
            Assert.Equal(expected, result);
    }

    private static Expression<Func<T, bool>> ReplaceParameter<T>(IEnumerable<T> enumerable, Expression<Func<List<T>, T, bool>> filter)
    {
        var para = Expression.Constant(enumerable);
        var paras = filter.Parameters;
        var newExp = ExpressionReplacer.Replace(filter.Body, paras.First(), para);
        var newParas = paras.Skip(1);
        var newFilter = Expression.Lambda<Func<T, bool>>(newExp, newParas);
        return newFilter;
    }
}