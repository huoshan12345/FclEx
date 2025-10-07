namespace FclEx.Domain;

public enum EntityChangeType
{
    None,
    Insert,
    Update,
    Delete,
}

public readonly record struct EntityChange<T>(T Entity, T? Existing, EntityChangeType ChangeType) where T : class
{
    public EntityChange ToNonGeneric() => new(Entity, Existing, ChangeType);
}

public readonly record struct EntityChange(object Entity, object? Existing, EntityChangeType ChangeType)
{
    public EntityChange<T> Cast<T>() where T : class => new((T)Entity, (T?)Existing, ChangeType);
    public static EntityChange Insert(object entity) => new(entity, default, EntityChangeType.Insert);
    public static EntityChange Update(object entity, object existing) => new(entity, existing, EntityChangeType.Update);
    public static EntityChange Delete(object entity) => new(entity, default, EntityChangeType.Delete);
}