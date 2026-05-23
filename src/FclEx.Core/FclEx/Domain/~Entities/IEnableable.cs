namespace FclEx.Domain;

/// <summary>
/// Represents an entity that can be disabled, tracking its disabled status.
/// </summary>
public interface IDisableable
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is disabled.
    /// </summary>
    bool IsDisabled { get; set; }
}