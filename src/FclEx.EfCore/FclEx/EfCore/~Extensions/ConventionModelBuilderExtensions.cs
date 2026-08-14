namespace FclEx.EfCore;

/// <summary>
/// Provides model-convention helpers for soft-delete-aware indexes.
/// </summary>
public static class ConventionModelBuilderExtensions
{
    private readonly record struct IndexAnnotation(string Name, object? Value, ConfigurationSource ConfigurationSource);

    /// <summary>
    /// Extends each unique index on an entity with any mapped soft-delete properties required by its implemented interfaces.
    /// </summary>
    /// <param name="modelBuilder">The convention model builder.</param>
    /// <param name="type">The entity type whose unique indexes should be inspected.</param>
    /// <returns>The same model builder.</returns>
    /// <remarks>
    /// Index names, uniqueness, sort order, configuration sources, and annotations are retained. Processing is skipped when
    /// <see cref="ConfigureSoftDeleteIndexesAttribute"/> is applied with <see langword="false"/>.
    /// </remarks>
    public static IConventionModelBuilder ConfigureSoftDeleteIndexes(this IConventionModelBuilder modelBuilder, IConventionEntityType type)
    {
        var clrType = type.ClrType;
        if (clrType.GetCustomAttribute<ConfigureSoftDeleteIndexesAttribute>()?.Enabled == false)
            return modelBuilder;

        var deletable = clrType.IsAssignableTo(typeof(ISoftDeletable));
        var hasDeleteAt = clrType.IsAssignableTo(typeof(IHasDeletedAt));

        if (deletable == false && hasDeleteAt == false)
            return modelBuilder;

        foreach (var index in type.GetIndexes().Where(index => index.IsUnique).ToArray())
        {
            var properties = index.Properties.ToList();

            AddProperty(nameof(ISoftDeletable.IsDeleted), deletable);
            AddProperty(nameof(IHasDeletedAt.DeletedAt), hasDeleteAt);

            var addedPropertyCount = properties.Count - index.Properties.Count;
            if (addedPropertyCount == 0)
                continue;

            var name = index.Name;
            var existingReplacement = type.GetIndexes().FirstOrDefault(candidate =>
                candidate != index &&
                candidate.Name == name &&
                candidate.Properties.Select(property => property.Name)
                    .SequenceEqual(properties.Select(property => property.Name)));

            if (existingReplacement?.IsUnique == true)
            {
                type.RemoveIndex(index);
                continue;
            }

            if (existingReplacement is not null)
                type.RemoveIndex(existingReplacement);

            var configurationSource = index.GetConfigurationSource();
            var isUniqueConfigurationSource = index.GetIsUniqueConfigurationSource();
            var isDescendingConfigurationSource = index.GetIsDescendingConfigurationSource();
            var isDescending = ExtendIsDescending(index.IsDescending, index.Properties.Count, addedPropertyCount);
            var annotations = index.GetAnnotations()
                .Select(annotation => new IndexAnnotation(
                    annotation.Name,
                    annotation.Value,
                    annotation.GetConfigurationSource()))
                .ToArray();
            type.RemoveIndex(index);

            var replacement = AddIndex(type, properties, name, configurationSource);
            SetIsUnique(replacement, true, isUniqueConfigurationSource ?? configurationSource);
            SetIsDescending(replacement, isDescending, isDescendingConfigurationSource);

            foreach (var annotation in annotations)
            {
                SetAnnotation(replacement, annotation);
            }

            void AddProperty(string propertyName, bool condition)
            {
                if (condition && properties.All(property => property.Name != propertyName))
                    properties.Add(type.GetProperty(propertyName));
            }
        }

        return modelBuilder;
    }

    private static IConventionIndex AddIndex(
        IConventionEntityType type,
        IReadOnlyList<IConventionProperty> properties,
        string? name,
        ConfigurationSource configurationSource)
    {
        if (configurationSource == ConfigurationSource.Explicit)
        {
            var mutableType = (IMutableEntityType)type;
            var mutableProperties = properties.Cast<IMutableProperty>().ToArray();
            return (IConventionIndex)(name is null
                ? mutableType.AddIndex(mutableProperties)
                : mutableType.AddIndex(mutableProperties, name));
        }

        var fromDataAnnotation = configurationSource == ConfigurationSource.DataAnnotation;
        return name is null
            ? type.AddIndex(properties, fromDataAnnotation)!
            : type.AddIndex(properties, name, fromDataAnnotation)!;
    }

    private static IReadOnlyList<bool>? ExtendIsDescending(
        IReadOnlyList<bool>? isDescending,
        int originalPropertyCount,
        int addedPropertyCount)
    {
        if (isDescending is null)
            return null;

        var result = isDescending.Count == 0
            ? Enumerable.Repeat(true, originalPropertyCount).ToList()
            : isDescending.ToList();

        result.AddRange(Enumerable.Repeat(false, addedPropertyCount));
        return result;
    }

    private static void SetIsUnique(IConventionIndex index, bool isUnique, ConfigurationSource configurationSource)
    {
        if (configurationSource == ConfigurationSource.Explicit)
        {
            ((IMutableIndex)index).IsUnique = isUnique;
        }
        else
        {
            index.SetIsUnique(isUnique, configurationSource == ConfigurationSource.DataAnnotation);
        }
    }

    private static void SetIsDescending(
        IConventionIndex index,
        IReadOnlyList<bool>? isDescending,
        ConfigurationSource? configurationSource)
    {
        if (configurationSource is null)
            return;

        if (configurationSource == ConfigurationSource.Explicit)
        {
            ((IMutableIndex)index).IsDescending = isDescending;
        }
        else
        {
            index.SetIsDescending(isDescending, configurationSource == ConfigurationSource.DataAnnotation);
        }
    }

    private static void SetAnnotation(IConventionIndex index, IndexAnnotation annotation)
    {
        if (annotation.ConfigurationSource == ConfigurationSource.Explicit)
        {
            ((IMutableIndex)index).SetAnnotation(annotation.Name, annotation.Value);
        }
        else
        {
            index.SetAnnotation(
                annotation.Name,
                annotation.Value,
                annotation.ConfigurationSource == ConfigurationSource.DataAnnotation);
        }
    }
}
