using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FclEx.EfCore;

public static class ChangeTrackerExtensions
{
    public static void HandleSoftDelete(this ChangeTracker tracker)
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

                    // 不允许更新为已删除，但是可以更新为未删除
                    if (entity is ISoftDeletable { IsDeleted: true })
                        exclude.Add(nameof(ISoftDeletable.IsDeleted));

                    if (entity is IHasDeletedAt)
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