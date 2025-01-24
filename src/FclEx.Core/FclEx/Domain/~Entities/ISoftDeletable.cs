namespace FclEx.Domain;

/// <summary>
/// Represents an entity that can be soft-deleted, where the deletion status is tracked without removing the entity from storage.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity has been soft-deleted.
    /// </summary>
    bool IsDeleted { get; set; }
}