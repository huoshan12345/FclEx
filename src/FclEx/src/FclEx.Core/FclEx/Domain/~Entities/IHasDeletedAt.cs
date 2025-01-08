namespace FclEx.Domain;

/// <summary>
/// Represents an entity that has a deletion timestamp.
/// </summary>
public interface IHasDeletedAt
{
    /// <summary>
    /// Gets or sets the timestamp indicating when the entity was deleted.
    /// </summary>
    DateTimeOffset DeletedAt { get; set; }
}