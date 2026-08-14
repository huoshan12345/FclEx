namespace FclEx.EfCore;

/// <summary>
/// Builds expression trees used by the query extensions in this package.
/// </summary>
public static class QueryableHelper
{
    private static readonly Expression EscapeChar = Expression.Constant(@"\");
    private static readonly Expression EscapedEscapeChar = Expression.Constant(@"\\");
    private static readonly Expression EfFunctions = Expression.Constant(EF.Functions);
    private static MethodInfo EfLike { get; } = typeof(DbFunctionsExtensions)
        .GetRequiredMethod(nameof(DbFunctionsExtensions.Like), 0, typeof(DbFunctions), typeof(string), typeof(string), typeof(string));

    internal static string GetContainsPattern(string value)
    {
        return GetContainsPattern(value, false);
    }

    internal static string GetContainsPattern(string value, bool escapeEscapeCharacter)
    {
        return $"%{EscapeLikePattern(value, escapeEscapeCharacter)}%";
    }

    private static string EscapeLikePattern(string value, bool escapeEscapeCharacter)
    {
        return value
            .Replace(@"\", escapeEscapeCharacter ? @"\\\\" : @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_")
            .Replace("[", @"\[");
    }

    /// <summary>
    /// Builds an EF Core <c>LIKE</c> predicate for a selected string member.
    /// </summary>
    /// <typeparam name="T">The queried entity type.</typeparam>
    /// <param name="selector">Selects the string member to compare.</param>
    /// <param name="pattern">The provider-ready LIKE pattern.</param>
    /// <param name="suppressValueConverter">
    /// Whether to suppress an EF Core value converter on the selected member by converting it through <see cref="object"/>.
    /// </param>
    /// <param name="escapeEscapeCharacter">
    /// Whether the provider requires the SQL escape character itself to be represented by two backslashes.
    /// </param>
    /// <returns>An expression that invokes <see cref="DbFunctionsExtensions.Like(DbFunctions, string, string, string)"/>.</returns>
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
        Expression patternExpression;
        if (escapeEscapeCharacter)
        {
            patternExpression = Expression.Constant(pattern, typeof(string));
        }
        else
        {
#if NET9_0_OR_GREATER
            Expression<Func<string>> patternParameter = () => EF.Parameter(pattern);
#else
            Expression<Func<string>> patternParameter = () => pattern;
#endif
            patternExpression = patternParameter.Body;
        }
        var escapeChar = escapeEscapeCharacter ? EscapedEscapeChar : EscapeChar;
        var call = Expression.Call(null, EfLike, EfFunctions, member, patternExpression, escapeChar);
        var where = Expression.Lambda<Func<T, bool>>(call, selector.Parameters);
        return where;
    }

    /// <summary>
    /// Builds a predicate that matches when the selected string contains at least one keyword.
    /// </summary>
    /// <typeparam name="T">The queried entity type.</typeparam>
    /// <param name="selector">Selects the string member to search.</param>
    /// <param name="keywords">The keywords to combine with logical OR. SQL LIKE metacharacters are treated literally.</param>
    /// <param name="suppressValueConverter">Whether to suppress an EF Core value converter on the selected member.</param>
    /// <param name="escapeEscapeCharacter">Whether the provider requires the SQL escape character itself to be escaped.</param>
    /// <returns>The combined predicate, or <see langword="null"/> when <paramref name="keywords"/> is empty.</returns>
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
            var pattern = GetContainsPattern(keyword, escapeEscapeCharacter);
            var expression = BuildLike(selector, pattern, suppressValueConverter, escapeEscapeCharacter);
            where = where.Or(expression);
        }
        return where;
    }

    /// <summary>
    /// Builds a predicate that compares an entity's <see cref="IHasId{T}.Id"/> property with a supplied key.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <param name="id">The key value to compare.</param>
    /// <returns>An expression equivalent to <c>entity =&gt; entity.Id == id</c>.</returns>
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
