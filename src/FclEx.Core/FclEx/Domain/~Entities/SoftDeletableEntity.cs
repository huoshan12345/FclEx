namespace FclEx.Domain;

/// <summary>
/// Represents an entity with support for soft deletion, including properties for tracking the deletion timestamp 
/// and deletion status. Inherits common entity properties such as an identifier, creation and update timestamps, 
/// and the ability to be disabled.
/// </summary>
/// <typeparam name="TPrimaryKey">The type of the primary key for the entity.</typeparam>
public class SoftDeletableEntity<TPrimaryKey> :
    CommonEntity<TPrimaryKey>,
    ISoftDeletableEntity<TPrimaryKey>
{
    public DateTimeOffset DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}


public class SoftDeletableEntity : SoftDeletableEntity<long>;