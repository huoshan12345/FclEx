namespace FclEx.Domain;

/// <summary>
/// Represents an update operation on an entity, holding both its new and existing versions.
/// </summary>
/// <typeparam name="T">The type of the entity being updated.</typeparam>
/// <param name="New">The updated version of the entity (after modification).</param>
/// <param name="Existing">The original version of the entity (before modification).</param>
/// <remarks>
/// For in-place updates, <see cref="New"/> and <see cref="Existing"/> may reference the same object.
/// </remarks>
public readonly record struct EntityUpdate<T>(T New, T Existing)
{
    /// <summary>
    /// Gets a value indicating whether the new and existing entities reference the same object instance.
    /// </summary>
    public bool IsSameInstance => ReferenceEquals(New, Existing);
}

/// <summary>
/// Represents a collection of entity changes, including inserted, updated, and deleted entities.
/// </summary>
/// <typeparam name="T">The type of the entity being tracked for changes.</typeparam>
public record EntityChanges<T>(
    List<T>? Inserted = null,
    List<EntityUpdate<T>>? Updated = null,
    List<T>? Deleted = null)
{
    /// <summary>
    /// Gets the list of newly inserted entities.
    /// </summary>
    public List<T> Inserted { get; init; } = Inserted ?? [];

    /// <summary>
    /// Gets the list of updated entities, represented as pairs of new and existing versions.
    /// </summary>
    public List<EntityUpdate<T>> Updated { get; init; } = Updated ?? [];

    /// <summary>
    /// Gets the list of deleted entities.
    /// </summary>
    public List<T> Deleted { get; init; } = Deleted ?? [];
}
