namespace FclEx.Domain;

/// <summary>
/// Represents a common implementation of an entity with a primary key, creation and update timestamps, 
/// and a soft delete flag, as well as the ability to be disabled.
/// </summary>
/// <typeparam name="TPrimaryKey">The type of the primary key for the entity.</typeparam>
public abstract class CommonEntity<TPrimaryKey> : ICommonEntity<TPrimaryKey>
{
    public TPrimaryKey Id { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsDisabled { get; set; }
}

public abstract class CommonEntity : CommonEntity<long>;