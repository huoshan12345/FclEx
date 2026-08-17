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
/// Represents an immutable snapshot of inserted, updated, and deleted entities.
/// </summary>
/// <typeparam name="T">The type of the entity being tracked for changes.</typeparam>
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
    /// Gets a snapshot of the newly inserted entities.
    /// </summary>
    public IReadOnlyList<T> Inserted { get; }

    /// <summary>
    /// Gets a snapshot of the updated entities, represented as pairs of new and existing versions.
    /// </summary>
    public IReadOnlyList<EntityUpdate<T>> Updated { get; }

    /// <summary>
    /// Gets a snapshot of the deleted entities.
    /// </summary>
    public IReadOnlyList<T> Deleted { get; }
}
