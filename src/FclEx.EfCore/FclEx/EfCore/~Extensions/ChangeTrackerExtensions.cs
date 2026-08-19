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
                    {
                        hasCreatedAt.CreatedAt = now;
                        entry.Property(nameof(IHasCreatedAt.CreatedAt)).IsModified = true;
                    }

                    if (entity is IHasUpdatedAt hasUpdatedAt)
                    {
                        hasUpdatedAt.UpdatedAt = now;
                        entry.Property(nameof(IHasUpdatedAt.UpdatedAt)).IsModified = true;
                    }

                    break;
                }
                case EntityState.Modified:
                {
                    if (entity is IHasUpdatedAt hasUpdatedAt)
                    {
                        hasUpdatedAt.UpdatedAt = now;
                        entry.Property(nameof(IHasUpdatedAt.UpdatedAt)).IsModified = true;
                    }

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
                        var property = entry.Property(nameof(IHasDeletedAt.DeletedAt));
                        if (restoringSoftDeletedEntity)
                        {
                            hasDeletedAt.DeletedAt = default; // Reset DeletedAt when restoring
                            property.IsModified = true;
                        }
                        else if (deletingSoftDeletedEntity && hasDeletedAt.DeletedAt == default)
                        {
                            // deleting but DeletedAt is not set, set it to now
                            hasDeletedAt.DeletedAt = now;
                            property.IsModified = true;
                        }
                        else if (isDeletable)
                        {
                            // If the entity is soft-deletable but not being deleted or restored, exclude DeletedAt from being modified
                            // updating IsDeleted from true to true.
                            property.IsModified = false;
                        }
                    }

                    if (entity is IHasCreatedAt)
                    {
                        entry.Property(nameof(IHasCreatedAt.CreatedAt)).IsModified = false;
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
                            property.IsModified = updatePropertyNames.Contains(property.Metadata.Name);
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
