namespace FclEx.Domain;

/// <summary>
/// Represents an entity that has an <see cref="UpdatedAt"/> property indicating the last update time.
/// </summary>
public interface IHasUpdatedAt
{
    /// <summary>
    /// Gets or sets the timestamp indicating when the entity was last updated.
    /// </summary>
    DateTimeOffset UpdatedAt { get; set; }
}