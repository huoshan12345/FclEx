namespace FclEx.Domain;

/// <summary>
/// Represents an entity that has a creation timestamp.
/// </summary>
public interface IHasCreatedAt
{
    /// <summary>
    /// Gets or sets the timestamp indicating when the entity was created.
    /// </summary>
    DateTimeOffset CreatedAt { get; set; }
}