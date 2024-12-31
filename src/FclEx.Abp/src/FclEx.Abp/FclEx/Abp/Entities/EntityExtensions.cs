using System;

namespace FclEx.Abp.Entities;

public static class EntityExtensions
{
    public static TEntity SetCreatedAt<TEntity>(this TEntity entity, DateTimeOffset time) where TEntity : class, IEntity
    {
        if (entity is IHasCreatedAt hasCreatedAt)
        {
            hasCreatedAt.CreatedAt = time;
        }
        return entity;
    }

    public static TEntity SetUpdatedAt<TEntity>(this TEntity entity, DateTimeOffset time) where TEntity : class, IEntity
    {
        if (entity is IHasUpdatedAt hasModificationTime)
        {
            hasModificationTime.UpdatedAt = time;
        }
        return entity;
    }

    public static TEntity SetDeletedAt<TEntity>(this TEntity entity, DateTimeOffset time) where TEntity : class, IEntity
    {
        if (entity is IHasDeletedAt hasDeletedAt)
        {
            hasDeletedAt.DeletedAt = time;
        }
        return entity;
    }
}