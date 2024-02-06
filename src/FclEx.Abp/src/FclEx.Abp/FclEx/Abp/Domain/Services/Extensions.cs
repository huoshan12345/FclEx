using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FclEx.Extensions;

namespace FclEx.Abp.Domain.Services;

public static class Extensions
{
    public static async Task<T?> GetAsync<T>(this IEntityService service, Expression<Func<T, bool>> filter)
    {
        var list = await service.GetListAsync(filter, 1).IgnoreSyncContext();
        return list.FirstOrDefault();
    }

    public static Task InsertAsync<T>(this IEntityService service, T entity)
    {
        return service.InsertListAsync(new[] { entity });
    }
}