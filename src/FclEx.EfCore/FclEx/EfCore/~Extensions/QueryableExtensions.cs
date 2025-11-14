namespace FclEx.EfCore;

public static class QueryableExtensions
{
    /// <summary>
    /// Retrieves an entity from the database by its primary key, optionally with no tracking.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
    /// <param name="queryable">The <see cref="DbSet{T}"/> to query.</param>
    /// <param name="id">The primary key value of the entity to retrieve.</param>
    /// <param name="noTracking">
    /// Whether the query should be executed with "no tracking" to optimize for read-only scenarios.
    /// Defaults to <c>true</c>.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="id"/> is null.</exception>
    public static Task<T?> GetAsync<T, TKey>(this IQueryable<T> queryable, TKey id, bool noTracking = true, CancellationToken cancellationToken = default)
        where T : class, IHasId<TKey>
    {
        if (id is null)
            return Task.FromResult(default(T?));

        var query = noTracking
            ? queryable.AsNoTracking()
            : queryable;

        var filter = QueryableHelper.BuildIdFilter<T, TKey>(id);
        return query.FirstOrDefaultAsync(filter, cancellationToken);
    }

    public static Task<T?> GetAsync<T>(this IQueryable<T> queryable, long id, bool noTracking = true, CancellationToken cancellationToken = default)
        where T : class, IHasId<long>
    {
        return queryable.GetAsync<T, long>(id, noTracking, cancellationToken);
    }

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

    /// <summary>
    /// Executes a bulk update operation on the given queryable source by applying
    /// the specified property values to all matching entities.
    ///
    /// This method dynamically builds an update expression that calls
    /// <c>SetProperty</c> for each entry in <paramref name="fieldValues"/> and then
    /// delegates to Entity Framework Core's <c>ExecuteUpdateAsync</c>.
    /// </summary>
    /// <typeparam name="T">
    /// The entity type of the query.
    /// </typeparam>
    /// <param name="query">
    /// The <see cref="IQueryable{T}"/> representing the set of entities to update.
    /// </param>
    /// <param name="fieldValues">
    /// A dictionary mapping property names to their new values. Each entry
    /// generates a corresponding <c>SetProperty</c> call in the update expression.
    /// Property names must match actual properties or fields on <typeparamref name="T"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that resolves to the number of state entries written to the database.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="query"/> or <paramref name="fieldValues"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if a property name in <paramref name="fieldValues"/> does not exist on
    /// <typeparamref name="T"/>.
    /// </exception>
    /// <remarks>
    /// This is a helper method that constructs the required update lambda for
    /// Entity Framework Core's batch update API at runtime. It supports simple
    /// member assignments and performs runtime conversion of supplied values to
    /// the appropriate property types.
    /// </remarks>
    public static Task<int> ExecuteUpdateAsync<T>(this IQueryable<T> query, IReadOnlyDictionary<string, object?> fieldValues, CancellationToken cancellationToken = default)
    {
        var updateBody = BuildUpdateBody(typeof(T), fieldValues);
        return (Task<int>)UpdateAsyncMethodInfo.MakeGenericMethod(query.ElementType)
            .Invoke(null, [query, updateBody, cancellationToken])!;
    }

    internal static LambdaExpression BuildUpdateBody(Type entityType, IReadOnlyDictionary<string, object?> fieldValues)
    {
#if NET10_0_OR_GREATER
        var setParam = Expression.Parameter(typeof(UpdateSettersBuilder<>).MakeGenericType(entityType), "s");
#else
        var setParam = Expression.Parameter(typeof(SetPropertyCalls<>).MakeGenericType(entityType), "s");
#endif

        var objParam = Expression.Parameter(entityType, "e");

        Expression setBody = setParam;
#if NET10_0_OR_GREATER
        const string methodName = nameof(UpdateSettersBuilder<>.SetProperty);
#else
        const string methodName = nameof(SetPropertyCalls<>.SetProperty);
#endif
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