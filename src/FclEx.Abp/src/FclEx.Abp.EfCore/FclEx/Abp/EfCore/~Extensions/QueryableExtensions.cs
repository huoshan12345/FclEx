using System.Linq;
using FclEx.Abp.Domain;

namespace FclEx.Abp.EfCore;

public static class QueryableExtensions
{
    public static IQueryable<T> NotDeleted<T>(this IQueryable<T> queryable) where T : IDeletable
    {
        return queryable.Where(m => m.IsDeleted == false);
    }

    public static IQueryable<T> Enabled<T>(this IQueryable<T> queryable) where T : IDisableable
    {
        return queryable.Where(m => m.IsDisabled == false);
    }

    public static IQueryable<T> Valid<T>(this IQueryable<T> queryable) where T : IDeletable, IDisableable
    {
        return queryable.NotDeleted().Enabled();
    }
}