namespace FclEx.EfCore;

public static class ConventionModelBuilderExtensions
{
    public static IConventionModelBuilder ConfigureSoftDeleteIndexes(this IConventionModelBuilder modelBuilder, IConventionEntityType type)
    {
        var clrType = type.ClrType;
        if (clrType.GetCustomAttribute<ConfigureSoftDeleteIndexesAttribute>()?.Enabled == false)
            return modelBuilder;

        var deletable = clrType.IsAssignableTo(typeof(ISoftDeletable));
        var hasDeleteAt = clrType.IsAssignableTo(typeof(IHasDeletedAt));

        if (deletable == false && hasDeleteAt == false)
            return modelBuilder;

        var add = new List<string[]>();
        var remove = new List<IConventionIndex>();

        foreach (var index in type.GetIndexes())
        {
            if (index.IsUnique == false)
                continue;

            var names = index.Properties.Select(m => m.Name).ToHashSet();
            var updated = false;

            if (deletable)
                updated = names.Add(nameof(ISoftDeletable.IsDeleted)) || updated;
            if (hasDeleteAt)
                updated = names.Add(nameof(IHasDeletedAt.DeletedAt)) || updated;

            if (updated == false)
                continue;

            add.Add(names.ToArray());
            remove.Add(index);
        }

        foreach (var index in remove)
        {
            type.RemoveIndex(index);
        }

        foreach (var names in add)
        {
            var properties = names.Select(type.GetProperty).ToArray();
            type.AddIndex(properties)!.SetIsUnique(true);
        }

        return modelBuilder;
    }
}
