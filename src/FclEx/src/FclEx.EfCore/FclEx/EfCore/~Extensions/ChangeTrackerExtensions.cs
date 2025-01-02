using FclEx.Domain;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FclEx.EfCore;

public static class ChangeTrackerExtensions
{
    public static void UpdateEntries(this ChangeTracker tracker)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in tracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                {
                    if (entry.Entity is IHasCreatedAt hasCreatedAt)
                    {
                        hasCreatedAt.CreatedAt = now;
                    }
                    if (entry.Entity is IHasUpdatedAt hasUpdatedAt)
                    {
                        hasUpdatedAt.UpdatedAt = now;
                    }
                    break;
                }
                case EntityState.Modified:
                {
                    if (entry.Entity is IHasUpdatedAt hasUpdatedAt)
                    {
                        hasUpdatedAt.UpdatedAt = now;
                    }
                    break;
                }
                case EntityState.Deleted:
                {
                    if (entry.Entity is IHasDeletedAt hasDeletedAt)
                    {
                        entry.State = EntityState.Modified;
                        hasDeletedAt.DeletedAt = now;
                    }
                    if (entry.Entity is ISoftDeletable deletable)
                    {
                        deletable.IsDeleted = true;
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