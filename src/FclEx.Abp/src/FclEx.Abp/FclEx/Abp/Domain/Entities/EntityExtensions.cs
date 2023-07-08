using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.ObjectMapping;

namespace FclEx.Abp.Domain.Entities;

public static class EntityExtensions
{
    public static TEntity SetModificationTime<TEntity>(this TEntity e, DateTime time)
        where TEntity : class, IEntity
    {
        Check.NotNull(e);
        if (e is IHasModificationTime hasModificationTime)
            hasModificationTime.LastModificationTime = time;
        return e;
    }

    public static TEntity SetModificationTimeToLocalNow<TEntity>(this TEntity e)
        where TEntity : class, IEntity
    {
        return e.SetModificationTime(DateTime.Now);
    }

    public static TEntity SetModificationTimeToUtcNow<TEntity>(this TEntity e)
        where TEntity : class, IEntity
    {
        return e.SetModificationTime(DateTime.UtcNow);
    }

    public static TEntity SetInitTime<TEntity>(this TEntity e, DateTime time)
        where TEntity : class, IEntity
    {
        Check.NotNull(e);
        if (e is IHasCreationTime hasCreationTime)
            hasCreationTime.CreationTime = time;
        if (e is IHasModificationTime hasModificationTime)
            hasModificationTime.LastModificationTime = time;
        return e;
    }

    public static TEntity SetInitTimeToLocalNow<TEntity>(this TEntity e)
        where TEntity : class, IEntity
    {
        return e.SetInitTime(DateTime.Now);
    }

    public static TEntity SetInitTimeToUtcNow<TEntity>(this TEntity e)
        where TEntity : class, IEntity
    {
        return e.SetInitTime(DateTime.UtcNow);
    }

    public static T Map<T>(this IEntity e, IObjectMapper mapper)
    {
        return mapper.Map<T>(e);
    }

    public static IEnumerable<TEntity> SetInitTimeToUtcNow<TEntity>(this IEnumerable<TEntity> enumerable)
        where TEntity : class, IEntity
    {
        var now = DateTime.UtcNow;
        return enumerable.Select(m => m.SetInitTime(now));
    }
}