namespace FclEx.Domain;

public static class EntityExtensions
{
    public static T SetCreatedAt<T>(this T entity, DateTimeOffset time) where T : IHasCreatedAt
    {
        entity.CreatedAt = time;
        return entity;
    }

    public static T SetUpdatedAt<T>(this T entity, DateTimeOffset time) where T : IHasUpdatedAt
    {
        entity.UpdatedAt = time;
        return entity;
    }

    public static T SetDeletedAt<T>(this T entity, DateTimeOffset time) where T : IHasDeletedAt
    {
        entity.DeletedAt = time;
        return entity;
    }
}