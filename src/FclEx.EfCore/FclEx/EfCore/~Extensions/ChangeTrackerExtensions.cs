namespace FclEx.EfCore;

/// <summary>
/// Provides state-transition rules for entities tracked by Entity Framework Core.
/// </summary>
public static class ChangeTrackerExtensions
{
    /// <summary>
    /// Applies state-specific rules to entities tracked by the <see cref="ChangeTracker"/>.
    /// This includes setting timestamps for created or updated entities, handling soft deletion, 
    /// and ensuring certain properties remain unchanged during state transitions.
    /// </summary>
    /// <param name="tracker">The <see cref="ChangeTracker"/> managing entity state transitions.</param>
    /// <remarks>
    /// For modified entities, the generated update timestamp is explicitly marked as modified and therefore remains
    /// persistent when <see cref="ChangeTracker.AutoDetectChangesEnabled"/> is disabled. Deleting an
    /// <see cref="ISoftDeletable"/> entity changes its state to <see cref="EntityState.Modified"/> and writes only its soft-delete members.
    /// </remarks>
    public static void ApplyEntityStateRules(this ChangeTracker tracker)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in tracker.Entries())
        {
            var entity = entry.Entity;
            switch (entry.State)
            {
                case EntityState.Added:
                {
                    if (entity is IHasCreatedAt hasCreatedAt)
                        hasCreatedAt.CreatedAt = now;

                    if (entity is IHasUpdatedAt hasUpdatedAt)
                        hasUpdatedAt.UpdatedAt = now;

                    break;
                }
                case EntityState.Modified:
                {
                    if (entity is IHasUpdatedAt hasUpdatedAt)
                        hasUpdatedAt.UpdatedAt = now;

                    var exclude = new HashSet<string>();

                    var isDeletable = false;
                    var deletingSoftDeletedEntity = false;
                    var restoringSoftDeletedEntity = false;
                    if (entity is ISoftDeletable deletable)
                    {
                        isDeletable = true;
                        if (deletable.IsDeleted)
                        {
                            if (Equals(entry.Property(nameof(ISoftDeletable.IsDeleted)).OriginalValue, false))
                            {
                                // update IsDeleted from false to true.
                                deletingSoftDeletedEntity = true;
                            }
                        }
                        else
                        {
                            // setting IsDeleted to false means the entity is being restored from a soft-deleted state
                            // updating IsDeleted from true/false to false.
                            restoringSoftDeletedEntity = true;
                        }
                    }

                    if (entity is IHasDeletedAt hasDeletedAt)
                    {
                        if (restoringSoftDeletedEntity)
                        {
                            hasDeletedAt.DeletedAt = default; // Reset DeletedAt when restoring
                        }
                        else if (deletingSoftDeletedEntity && hasDeletedAt.DeletedAt == default)
                        {
                            // deleting but DeletedAt is not set, set it to now
                            hasDeletedAt.DeletedAt = now;
                        }
                        else if (isDeletable)
                        {
                            // If the entity is soft-deletable but not being deleted or restored, exclude DeletedAt from being modified
                            // updating IsDeleted from true to true.
                            exclude.Add(nameof(IHasDeletedAt.DeletedAt));
                        }
                    }

                    if (entity is IHasCreatedAt)
                        exclude.Add(nameof(IHasCreatedAt.CreatedAt));

                    foreach (var name in exclude)
                    {
                        entry.Property(name).IsModified = false;
                    }

                    break;
                }
                case EntityState.Deleted:
                {
                    if (entity is ISoftDeletable deletable)
                    {
                        entry.State = EntityState.Modified;
                        deletable.IsDeleted = true;

                        var updatePropertyNames = new HashSet<string> { nameof(ISoftDeletable.IsDeleted) };

                        if (entity is IHasDeletedAt hasDeletedAt)
                        {
                            hasDeletedAt.DeletedAt = now;
                            updatePropertyNames.Add(nameof(IHasDeletedAt.DeletedAt));
                        }

                        foreach (var property in entry.Properties)
                        {
                            if (updatePropertyNames.Contains(property.Metadata.Name))
                                continue;

                            property.IsModified = false;
                        }
                    }
                    break;
                }
                case EntityState.Detached:
                case EntityState.Unchanged:
                default:
                    break;
            }
        }
    }
}
