namespace FclEx.Domain;

/// <summary>
/// Represents a common entity with standard properties and behaviors, including an identifier, creation and update timestamps,
/// and a soft delete flag, as well as the ability to be disabled.
/// </summary>
/// <typeparam name="TPrimaryKey">The type of the primary key for the entity.</typeparam>
public interface ICommonEntity<TPrimaryKey> :
    IHasId<TPrimaryKey>,
    IHasCreatedAt,
    IHasUpdatedAt,
    IDisableable;