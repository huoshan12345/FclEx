namespace FclEx.EfCore;

public static class QueryableExtensions
{
    private static async Task<(T[] items, int TotalCount)> ToArrayAndCountAsync<T>(this IQueryable<T> queryable, int pageSize, int pageIndex)
    {
        var count = await queryable.CountAsync();
        if (count == 0)
            return ([], 0);

        var items = await queryable
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        return (items, count);
    }

    public static async Task<PagedListModel<T>> ToPagedListAsync<T>(this IQueryable<T> queryable, int pageSize, int pageIndex)
    {
        var (items, count) = await queryable.ToArrayAndCountAsync(pageSize, pageIndex);
        return new(new PagedList<T>(items, pageIndex, pageSize, count));
    }

    public static async Task<PagedListModel<TModel>> ToPagedListAsync<T, TModel>(this IQueryable<T> queryable, int pageSize, int pageIndex, Func<T, TModel> selector)
    {
        var (items, count) = await queryable.ToArrayAndCountAsync(pageSize, pageIndex);
        var arr = items.Select(selector).ToArray();
        return new(new PagedList<TModel>(arr, pageIndex, pageSize, count));
    }

    public static IQueryable<T> ContainsAny<T>(this IQueryable<T> queryable, Expression<Func<T, string?>> selector, IEnumerable<string> keywords, bool suppressValueConverter = false)
    {
        Expression<Func<T, bool>>? where = null;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var keyword in keywords)
        {
            var pattern = QueryableHelper.GetContainsPattern(keyword);
            var expression = QueryableHelper.BuildLike(selector, pattern, suppressValueConverter);
            where = where.Or(expression);
        }
        return where == null ? queryable : queryable.Where(where);
    }

    public static IQueryable<T> NotDeleted<T>(this IQueryable<T> queryable) where T : ISoftDeletable
    {
        return queryable.Where(m => m.IsDeleted == false);
    }

    public static IQueryable<T> Enabled<T>(this IQueryable<T> queryable) where T : IDisableable
    {
        return queryable.Where(m => m.IsDisabled == false);
    }

    public static IQueryable<T> Valid<T>(this IQueryable<T> queryable) where T : ISoftDeletable, IDisableable
    {
        return queryable.NotDeleted().Enabled();
    }

    public static Task<int> ExecuteSoftDeleteAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        var type = typeof(T);
        var deletable = type.IsAssignableTo(typeof(ISoftDeletable));
        var hasDeleteAt = type.IsAssignableTo(typeof(IHasDeletedAt));

        if (!deletable || !hasDeleteAt)
            return query.ExecuteDeleteAsync(cancellationToken);

        var values = new Dictionary<string, object?>();

        if (deletable)
            values.Add(nameof(ISoftDeletable.IsDeleted), true);

        if (hasDeleteAt)
            values.Add(nameof(IHasDeletedAt.DeletedAt), DateTimeOffset.UtcNow);

        return query.ExecuteUpdateAsync(values, cancellationToken);
    }

#if NET9_0_OR_GREATER // the method has moved since EfCore 9.0
    private static readonly MethodInfo UpdateAsyncMethodInfo = typeof(EntityFrameworkQueryableExtensions)
        .GetRequiredMethod(nameof(EntityFrameworkQueryableExtensions.ExecuteUpdateAsync));
#else
    private static readonly MethodInfo UpdateAsyncMethodInfo = typeof(RelationalQueryableExtensions)
        .GetRequiredMethod(nameof(RelationalQueryableExtensions.ExecuteUpdateAsync));
#endif

    public static Task<int> ExecuteUpdateAsync<T>(this IQueryable<T> query, IReadOnlyDictionary<string, object?> fieldValues, CancellationToken cancellationToken = default)
    {
        var updateBody = BuildUpdateBody(typeof(T), fieldValues);
        return (Task<int>)UpdateAsyncMethodInfo.MakeGenericMethod(query.ElementType)
            .Invoke(null, [query, updateBody, cancellationToken])!;
    }

    internal static LambdaExpression BuildUpdateBody(Type entityType, IReadOnlyDictionary<string, object?> fieldValues)
    {
        var setParam = Expression.Parameter(typeof(SetPropertyCalls<>).MakeGenericType(entityType), "s");
        var objParam = Expression.Parameter(entityType, "e");

        Expression setBody = setParam;

        const string methodName = nameof(SetPropertyCalls<object>.SetProperty);
        foreach (var pair in fieldValues)
        {
            var propExpression = Expression.PropertyOrField(objParam, pair.Key);
            var valueExpression = ValueForType(propExpression.Type, pair.Value);

            // s.SetProperty(e => e.SomeField, value)
            var lambda = Expression.Lambda(propExpression, objParam);
            setBody = Expression.Call(setBody, methodName, [propExpression.Type], lambda, valueExpression);

        }

        // s => s.SetProperty(e => e.SomeField, value)
        var updateBody = Expression.Lambda(setBody, setParam);

        return updateBody;
    }

    internal static Expression ValueForType(Type desiredType, object? value)
    {
        if (value == null)
        {
            return Expression.Default(desiredType);
        }

        if (value.GetType() != desiredType)
        {
            return Expression.Convert(Expression.Constant(value), desiredType);
        }

        return Expression.Constant(value);
    }
}