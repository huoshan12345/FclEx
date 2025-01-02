using FclEx.Domain;

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
}