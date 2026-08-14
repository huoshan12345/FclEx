using Microsoft.EntityFrameworkCore.ChangeTracking;

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
                    {
                        hasUpdatedAt.UpdatedAt = now;
                        entry.Property(nameof(IHasUpdatedAt.UpdatedAt)).IsModified = true;
                    }

                    var exclude = new HashSet<string>();

                    var restoringSoftDeletedEntity = false;
                    if (entity is ISoftDeletable deletable)
                    {
                        // Ignore direct updates to true, but persist the false transition used to restore an entity.
                        if (deletable.IsDeleted)
                        {
                            exclude.Add(nameof(ISoftDeletable.IsDeleted));
                        }
                        else
                        {
                            restoringSoftDeletedEntity = Equals(entry.Property(nameof(ISoftDeletable.IsDeleted)).OriginalValue, true);
                        }
                    }

                    if (entity is IHasDeletedAt && restoringSoftDeletedEntity == false)
                        exclude.Add(nameof(IHasDeletedAt.DeletedAt));

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

                        var exclude = new HashSet<string> { nameof(ISoftDeletable.IsDeleted) };

                        if (entity is IHasDeletedAt hasDeletedAt)
                        {
                            hasDeletedAt.DeletedAt = now;
                            exclude.Add(nameof(IHasDeletedAt.DeletedAt));
                        }

                        foreach (var property in entry.Properties)
                        {
                            if (exclude.Contains(property.Metadata.Name))
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
