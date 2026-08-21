namespace FclEx.Domain;

/// <summary>
/// Represents an update operation, holding the entity to which the update was applied and the matched existing entity.
/// </summary>
/// <typeparam name="T">The type of the entity being updated.</typeparam>
/// <param name="New">The entity representing the applied update.</param>
/// <param name="Existing">The entity that was matched in the existing input.</param>
/// <remarks>
/// <see cref="Existing"/> is not a snapshot of values before the update. For an in-place update it may already
/// contain updated values and may reference the same object as <see cref="New"/>.
/// </remarks>
public readonly record struct EntityUpdate<T>(T New, T Existing)
{
    /// <summary>
    /// Gets a value indicating whether the new and existing entities reference the same object instance.
    /// </summary>
    public bool IsSameInstance => ReferenceEquals(New, Existing);
}

/// <summary>
/// Represents an immutable snapshot of the entity references produced by insert, update, and delete operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <remarks>
/// The collections are copied and exposed as read-only lists, but the contained entities are not cloned and remain mutable.
/// The operation that creates this result defines which concrete entity instances represent the applied changes.
/// </remarks>
public sealed class EntityChanges<T>
{
    /// <summary>
    /// Creates a snapshot by copying the supplied change sequences.
    /// </summary>
    /// <param name="inserted">The inserted entities, or <see langword="null"/> for an empty sequence.</param>
    /// <param name="updated">The entity updates, or <see langword="null"/> for an empty sequence.</param>
    /// <param name="deleted">The deleted entities, or <see langword="null"/> for an empty sequence.</param>
    public EntityChanges(
        IEnumerable<T>? inserted = null,
        IEnumerable<EntityUpdate<T>>? updated = null,
        IEnumerable<T>? deleted = null)
    {
        Inserted = Array.AsReadOnly(inserted?.ToArray() ?? []);
        Updated = Array.AsReadOnly(updated?.ToArray() ?? []);
        Deleted = Array.AsReadOnly(deleted?.ToArray() ?? []);
    }

    /// <summary>
    /// Gets the entities representing the applied insert operations.
    /// </summary>
    public IReadOnlyList<T> Inserted { get; }

    /// <summary>
    /// Gets the applied update entities paired with the entities matched in the existing input.
    /// </summary>
    public IReadOnlyList<EntityUpdate<T>> Updated { get; }

    /// <summary>
    /// Gets the entities representing the applied delete operations.
    /// </summary>
    public IReadOnlyList<T> Deleted { get; }
}
