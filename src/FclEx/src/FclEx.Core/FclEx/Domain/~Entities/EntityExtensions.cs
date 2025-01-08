namespace FclEx.Domain;

public static class EntityExtensions
{
    public static T SetCreatedAt<T>(this T entity, DateTimeOffset time)
    {
        if (entity is IHasCreatedAt hasCreatedAt)
        {
            hasCreatedAt.CreatedAt = time;
        }
        return entity;
    }

    public static T SetUpdatedAt<T>(this T entity, DateTimeOffset time)
    {
        if (entity is IHasUpdatedAt hasModificationTime)
        {
            hasModificationTime.UpdatedAt = time;
        }
        return entity;
    }

    public static T SetDeletedAt<T>(this T entity, DateTimeOffset time)
    {
        if (entity is IHasDeletedAt hasDeletedAt)
        {
            hasDeletedAt.DeletedAt = time;
        }
        return entity;
    }
}