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

    private static readonly Expression EscapeChar = Expression.Constant(@"\");
    private static readonly Expression EscapedEscapeChar = Expression.Constant(@"\\");

    public static Expression<Func<T, bool>> BuildLike<T>(
        Expression<Func<T, string?>> selector,
        string pattern,
        bool suppressValueConverter,
        bool escapeEscapeCharacter)
    {
        var member = selector.Body;
        if (suppressValueConverter)
        {
            var convertToObject = Expression.Convert(selector.Body, typeof(object));
            member = Expression.Convert(convertToObject, typeof(string));
        }
        var expPattern = Expression.Constant(pattern, typeof(string));
        var escapeChar = escapeEscapeCharacter ? EscapedEscapeChar : EscapeChar;
        var call = Expression.Call(null, EfLike, EfFunctions, member, expPattern, escapeChar);
        var where = Expression.Lambda<Func<T, bool>>(call, selector.Parameters);
        return where;
    }

    public static Expression<Func<T, bool>>? BuildContainsAny<T>(
        Expression<Func<T, string?>> selector,
        IEnumerable<string> keywords,
        bool suppressValueConverter = false,
        bool escapeEscapeCharacter = false)
    {
        Expression<Func<T, bool>>? where = null;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var keyword in keywords)
        {
            var pattern = GetContainsPattern(keyword);
            var expression = BuildLike(selector, pattern, suppressValueConverter, escapeEscapeCharacter);
            where = where.Or(expression);
        }
        return where;
    }

    public static Expression<Func<T, bool>> BuildFilter<T>(IEnumerable<IIndex> indexes, T entity) where T : notnull
    {
        // m => m.Property == value || m.Property2 == value2
        var objParam = Expression.Parameter(typeof(T));

        Expression? conditions = null;
        foreach (var index in indexes)
        {
            Check.NotEmpty(index.Properties);

            Expression? condition = null;
            foreach (var property in index.Properties)
            {
                var member = Expression.PropertyOrField(objParam, property.Name);
                var value = Expression.Constant(property.GetGetter().GetClrValue(entity));
                var equal = Expression.Equal(member, value);
                condition = condition is null ? equal : Expression.Add(condition, equal);
            }

            Check.NotNull(condition);
            conditions = conditions is null ? condition : Expression.OrElse(conditions, condition);
        }

        Check.NotNull(conditions);
        var lambda = Expression.Lambda<Func<T, bool>>(conditions, objParam);
        return lambda;
    }

    public static Expression<Func<T, bool>> BuildIdFilter<T, TKey>(TKey id) where T : IHasId<TKey>
    {
        // m => m.Id == id
        var objParam = Expression.Parameter(typeof(T));
        var member = Expression.Property(objParam, nameof(IHasId<>.Id));
        var value = Expression.Constant(id);
        var equal = Expression.Equal(member, value);
        var lambda = Expression.Lambda<Func<T, bool>>(equal, objParam);
        return lambda;
    }
}