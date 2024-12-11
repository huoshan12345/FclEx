namespace FclEx.EfCore;

public static class QueryableHelper
{
    public static readonly Expression EfFunctions = Expression.Constant(EF.Functions);
    public static MethodInfo ContainsOfString { get; } = typeof(string).GetRequiredMethod(nameof(string.Contains), 0, typeof(string));

    public static MethodInfo EfLike { get; } = typeof(DbFunctionsExtensions)
        .GetRequiredMethod(nameof(DbFunctionsExtensions.Like), 0, typeof(DbFunctions), typeof(string), typeof(string), typeof(string));

    private static readonly ConcurrentDictionary<string, string> _contains = new();
    public static string GetContainsPattern(string value)
    {
        return _contains.GetOrAdd(value, m => $"%{m.Replace("%", @"\%")}%");
    }

    public static Expression<Func<T, bool>> BuildLike<T>(Expression<Func<T, string?>> selector, string pattern, bool suppressValueConverter)
    {
        var member = selector.Body;
        if (suppressValueConverter)
        {
            var convertToObject = Expression.Convert(selector.Body, typeof(object));
            member = Expression.Convert(convertToObject, typeof(string));
        }
        var expPattern = Expression.Constant(pattern, typeof(string));
        var call = Expression.Call(null, EfLike, EfFunctions, member, expPattern, Expression.Constant("\\"));
        var where = Expression.Lambda<Func<T, bool>>(call, selector.Parameters);
        return where;
    }

    public static Expression<Func<T, bool>>? BuildContainsAny<T>(Expression<Func<T, string?>> selector, IEnumerable<string> keywords, bool suppressValueConverter = false)
    {
        Expression<Func<T, bool>>? where = null;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var keyword in keywords)
        {
            var pattern = GetContainsPattern(keyword);
            var expression = BuildLike(selector, pattern, suppressValueConverter);
            where = where.Or(expression);
        }
        return where;
    }
}