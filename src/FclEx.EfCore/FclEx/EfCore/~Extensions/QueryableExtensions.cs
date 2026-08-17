namespace FclEx.EfCore;

/// <summary>
/// Provides retrieval, paging, filtering, and set-based mutation operations for EF Core queries.
/// </summary>
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
    /// Defaults to <see langword="true"/>.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity if found; otherwise, <see langword="null"/>.</returns>
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

    /// <summary>
    /// Retrieves an entity with a <see cref="long"/> primary key, optionally without tracking it.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="queryable">The query to search.</param>
    /// <param name="id">The primary key value.</param>
    /// <param name="noTracking">Whether to disable change tracking for the query.</param>
    /// <param name="cancellationToken">A token to observe while executing the query.</param>
    /// <returns>The matching entity, or <see langword="null"/> when no entity exists.</returns>
    public static Task<T?> GetAsync<T>(this IQueryable<T> queryable, long id, bool noTracking = true, CancellationToken cancellationToken = default)
        where T : class, IHasId<long>
    {
        return queryable.GetAsync<T, long>(id, noTracking, cancellationToken);
    }

    private static async Task<(T[] items, int TotalCount)> ToArrayAndCountAsync<T>(
        this IQueryable<T> queryable,
        int pageSize,
        int pageIndex,
        bool noTracking = true,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var offset = GetPageOffset(pageSize, pageIndex);
        var query = noTracking
            ? queryable.AsNoTracking()
            : queryable;

        var count = await query.CountAsync(cancellationToken);
        if (count == 0)
            return ([], 0);

        var items = await query
            .Skip(offset)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return (items, count);
    }

    /// <summary>
    /// Materializes one page of entities and counts all matching rows.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="queryable">The query to count and page.</param>
    /// <param name="pageSize">The maximum number of entities in the page.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="noTracking">Whether to disable change tracking before materialization.</param>
    /// <param name="cancellationToken">A token to observe while executing the count and page queries.</param>
    /// <returns>The requested page and total number of matching rows.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageSize"/> is less than one, or the page index or resulting offset is invalid.</exception>
    public static async Task<PagedListModel<T>> ToPagedListAsync<T>(
        this IQueryable<T> queryable,
        int pageSize,
        int pageIndex,
        bool noTracking = true,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var (items, count) = await queryable.ToArrayAndCountAsync(pageSize, pageIndex, noTracking, cancellationToken);
        return new(new PagedList<T>(items, pageIndex, pageSize, count));
    }

    /// <summary>
    /// Materializes one page of entities, projects it in memory, and counts all matching rows.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TModel">The projected result type.</typeparam>
    /// <param name="queryable">The query to count and page.</param>
    /// <param name="pageSize">The maximum number of entities in the page.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="selector">The in-memory projection applied after entities are materialized.</param>
    /// <param name="noTracking">Whether to disable change tracking before materialization.</param>
    /// <param name="cancellationToken">A token to observe while executing the count and page queries.</param>
    /// <returns>The projected page and total number of matching rows.</returns>
    /// <remarks>Use the expression-based overload to translate projection into SQL and avoid materializing full entities.</remarks>
    public static async Task<PagedListModel<TModel>> ToPagedListAsync<T, TModel>(
        this IQueryable<T> queryable,
        int pageSize,
        int pageIndex,
        Func<T, TModel> selector,
        bool noTracking = true,
        CancellationToken cancellationToken = default) where T : class
    {
        var (items, count) = await queryable.ToArrayAndCountAsync(pageSize, pageIndex, noTracking, cancellationToken);
        var arr = items.Select(selector).ToArray();
        return new(new PagedList<TModel>(arr, pageIndex, pageSize, count));
    }

    /// <summary>
    /// Creates a page by projecting matching entities in the database before materialization.
    /// </summary>
    /// <typeparam name="T">The entity type being queried.</typeparam>
    /// <typeparam name="TModel">The projected result type.</typeparam>
    /// <param name="queryable">The query to count, page, and project.</param>
    /// <param name="selector">An expression translated by the database provider to select the required values.</param>
    /// <param name="pageSize">The maximum number of items in the page.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="noTracking">Whether entity tracking should be disabled before applying the projection.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the queries to complete.</param>
    /// <returns>The projected page and the total number of matching rows.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageSize"/> is less than one, or the page index or resulting offset is invalid.</exception>
    public static async Task<PagedListModel<TModel>> ToPagedListAsync<T, TModel>(
        this IQueryable<T> queryable,
        Expression<Func<T, TModel>> selector,
        int pageSize,
        int pageIndex,
        bool noTracking = true,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var offset = GetPageOffset(pageSize, pageIndex);
        var query = noTracking
            ? queryable.AsNoTracking()
            : queryable;

        var count = await query.CountAsync(cancellationToken);
        if (count == 0)
            return new(new PagedList<TModel>([], pageIndex, pageSize, 0));

        var items = await query
            .Skip(offset)
            .Take(pageSize)
            .Select(selector)
            .ToArrayAsync(cancellationToken);

        return new(new PagedList<TModel>(items, pageIndex, pageSize, count));
    }

    private static int GetPageOffset(int pageSize, int pageIndex)
    {
        Check.NotLessThan(pageSize, 1);
        Check.NotLessThan(pageIndex, 0);

        if (pageIndex > (int.MaxValue - 1) / pageSize)
            throw new ArgumentOutOfRangeException(nameof(pageIndex), pageIndex, "The page offset is too large.");

        return pageIndex * pageSize;
    }

    /// <summary>
    /// Filters the query to rows whose selected string contains at least one keyword.
    /// SQL LIKE metacharacters in keywords are treated as literal characters.
    /// </summary>
    /// <param name="queryable">The query to filter.</param>
    /// <param name="selector">Selects the string column to search.</param>
    /// <param name="keywords">The keywords to search for. An empty sequence produces a query with no matches.</param>
    /// <param name="suppressValueConverter">Whether to suppress an EF Core value converter on the selected member.</param>
    /// <param name="escapeEscapeCharacter">
    /// Whether the SQL escape character must itself be escaped by the provider. Set this to <see langword="true"/>
    /// for Oracle's MySql.EntityFrameworkCore provider; leave it <see langword="false"/> for SQL Server,
    /// PostgreSQL, and MySqlConnector-based providers.
    /// </param>
    public static IQueryable<T> ContainsAny<T>(
        this IQueryable<T> queryable,
        Expression<Func<T, string?>> selector,
        IEnumerable<string> keywords,
        bool suppressValueConverter = false,
        bool escapeEscapeCharacter = false)
    {
        var where = QueryableHelper.BuildContainsAny(selector, keywords, suppressValueConverter, escapeEscapeCharacter);
        return where == null
            ? queryable.Where(m => false)
            : queryable.Where(where);
    }

    /// <summary>
    /// Filters a query to entities that have not been soft-deleted.
    /// </summary>
    /// <typeparam name="T">The soft-deletable entity type.</typeparam>
    /// <param name="queryable">The query to filter.</param>
    /// <returns>A query constrained to rows whose <see cref="ISoftDeletable.IsDeleted"/> value is <see langword="false"/>.</returns>
    public static IQueryable<T> NotDeleted<T>(this IQueryable<T> queryable) where T : ISoftDeletable
    {
        return queryable.Where(m => m.IsDeleted == false);
    }

    /// <summary>
    /// Filters a query to entities that are enabled.
    /// </summary>
    /// <typeparam name="T">The disableable entity type.</typeparam>
    /// <param name="queryable">The query to filter.</param>
    /// <returns>A query constrained to rows whose <see cref="IDisableable.IsDisabled"/> value is <see langword="false"/>.</returns>
    public static IQueryable<T> Enabled<T>(this IQueryable<T> queryable) where T : IDisableable
    {
        return queryable.Where(m => m.IsDisabled == false);
    }

    /// <summary>
    /// Filters a query to entities that are neither soft-deleted nor disabled.
    /// </summary>
    /// <typeparam name="T">An entity type supporting soft deletion and disabling.</typeparam>
    /// <param name="queryable">The query to filter.</param>
    /// <returns>A query containing only active entities.</returns>
    public static IQueryable<T> Valid<T>(this IQueryable<T> queryable) where T : ISoftDeletable, IDisableable
    {
        return queryable.NotDeleted().Enabled();
    }

    /// <summary>
    /// Deletes matching rows, using a bulk update for soft-deletable entities and a bulk delete otherwise.
    /// </summary>
    /// <remarks>
    /// The operation executes directly in the database and does not update entities already held by the change tracker.
    /// Reload or detach tracked instances before relying on their state after this operation.
    /// </remarks>
    /// <typeparam name="T">The queried entity type.</typeparam>
    /// <param name="query">The rows to delete.</param>
    /// <param name="cancellationToken">A token to observe while executing the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static Task<int> ExecuteSoftDeleteAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        var type = typeof(T);
        var deletable = type.IsAssignableTo(typeof(ISoftDeletable));

        if (deletable == false)
            return query.ExecuteDeleteAsync(cancellationToken);

        var hasDeleteAt = type.IsAssignableTo(typeof(IHasDeletedAt));
        var values = new Dictionary<string, object?>();

        if (deletable)
        {
            var entity = Expression.Parameter(type, "entity");
            var isDeleted = Expression.Property(entity, nameof(ISoftDeletable.IsDeleted));
            var notDeleted = Expression.Lambda<Func<T, bool>>(
                Expression.Equal(isDeleted, Expression.Constant(false)), entity);
            query = query.Where(notDeleted);
            values.Add(nameof(ISoftDeletable.IsDeleted), true);
        }

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
    /// <typeparamref name="T"/>, or if <see langword="null"/> is assigned to a non-nullable value type.
    /// </exception>
    /// <remarks>
    /// This is a helper method that constructs the required update lambda for
    /// Entity Framework Core's batch update API at runtime. It supports simple member assignments. Values whose runtime
    /// type differs from the target member are represented with an expression-tree conversion; the CLR types must support
    /// that conversion. An empty dictionary returns zero without executing a command. The operation executes directly in
    /// the database and does not update entities already held by the change tracker.
    /// </remarks>
    public static Task<int> ExecuteUpdateAsync<T>(this IQueryable<T> query, IReadOnlyDictionary<string, object?> fieldValues, CancellationToken cancellationToken = default)
    {
        Check.NotNull(query);
        Check.NotNull(fieldValues);

        if (fieldValues.Count == 0)
            return Task.FromResult(0);

        var updateBody = BuildUpdateBody(typeof(T), fieldValues);
        object param = updateBody;

#if NET10_0_OR_GREATER
        var block = Expression.Block(
            updateBody.Body,
            Expression.Empty() // Explicitly return void (or do nothing)
        );
        var lambda = Expression.Lambda(block, updateBody.Parameters);
        param = lambda.Compile();
#endif
        return (Task<int>)UpdateAsyncMethodInfo.MakeGenericMethod(query.ElementType)
            .Invoke(null, [query, param, cancellationToken])!;
    }

    internal static LambdaExpression BuildUpdateBody(Type entityType, IReadOnlyDictionary<string, object?> fieldValues)
    {
#if NET10_0_OR_GREATER
        var setParam = Expression.Parameter(typeof(UpdateSettersBuilder<>).MakeGenericType(entityType));
#else
        var setParam = Expression.Parameter(typeof(SetPropertyCalls<>).MakeGenericType(entityType));
#endif
        var objParam = Expression.Parameter(entityType);

        Expression setBody = setParam;
#if NET10_0_OR_GREATER
        const string methodName = nameof(UpdateSettersBuilder<>.SetProperty);
#else
        const string methodName = nameof(SetPropertyCalls<>.SetProperty);
#endif
        foreach (var (key, value) in fieldValues)
        {
            var propExpression = Expression.PropertyOrField(objParam, key);
            var valueExpression = ValueForType(propExpression.Type, value);

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
            if (desiredType.IsValueType && Nullable.GetUnderlyingType(desiredType) is null)
                throw new ArgumentException($"Null cannot be assigned to non-nullable type '{desiredType}'.", nameof(value));

            return Expression.Default(desiredType);
        }

        if (value.GetType() != desiredType)
        {
            return Expression.Convert(Expression.Constant(value), desiredType);
        }

        return Expression.Constant(value);
    }
}
