namespace FclEx.Domain;

/// <summary>
/// Represents an entity that supports soft deletion, with additional properties to track the deletion timestamp 
/// and status, while inheriting common entity properties such as an identifier, creation and update timestamps, 
/// and the ability to be disabled.
/// </summary>
/// <typeparam name="TPrimaryKey">The type of the primary key for the entity.</typeparam>
public interface ISoftDeletableEntity<TPrimaryKey> :
    ICommonEntity<TPrimaryKey>,
    IHasDeletedAt,
    ISoftDeletable;
