namespace FclEx.Domain;

public enum EntityChangeType
{
    None,
    Insert,
    Update,
    Delete,
}

public readonly record struct EntityChange<T>(T Entity, EntityChangeType ChangeType);