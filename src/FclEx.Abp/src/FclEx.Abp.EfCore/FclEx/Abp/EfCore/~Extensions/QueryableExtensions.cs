using System.Linq;

namespace FclEx.Abp.EfCore;

public static class QueryableExtensions
{
    public static IQueryable<T> Undeleted<T>(this IQueryable<T> queryable) where T : ISoftDelete
    {
        return queryable.Where(m => !m.IsDeleted);
    }

    public static IQueryable<T> Active<T>(this IQueryable<T> queryable) where T : IPassivable
    {
        return queryable.Where(m => m.IsActive);
    }

    public static IQueryable<T> Valid<T>(this IQueryable<T> queryable) where T : ISoftDelete, IPassivable
    {
        return queryable.Undeleted().Active();
    }
}