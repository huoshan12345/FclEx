namespace FclEx.Domain;

/// <summary>
/// Represents an entity that has an identifiable primary key.
/// </summary>
/// <typeparam name="T">The type of the identifier.</typeparam>
/// <remarks>
/// This interface can be implemented by classes to standardize access to an entity's unique identifier.
/// It is useful for generic programming, where operations like fetching, updating, or deleting 
/// can be performed on entities with a common identifier property.
/// </remarks>
public interface IHasId<T>
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    T Id { get; set; }
}
