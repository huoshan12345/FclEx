namespace FclEx.Domain;

/// <summary>Creates comparers that detect changes in entity data rather than comparing entity identity alone.</summary>
public static class EntityDataEqualityComparer
{
    /// <summary>Gets the default member names excluded from entity data comparison: CreatedAt, UpdatedAt, and DeletedAt.</summary>
    /// <remarks>
    /// The default names cannot be modified. Pass a complete exclusion list to Create to use a different policy.
    /// </remarks>
    public static readonly ReadOnlyHashSet<string> DefaultExcludedNames = new HashSet<string>
    {
        nameof(IHasCreatedAt.CreatedAt),
        nameof(IHasUpdatedAt.UpdatedAt),
        nameof(IHasDeletedAt.DeletedAt),
    }.AsReadOnlyHashSet();

    /// <summary>Creates an entity data comparer using the complete set of excluded member names.</summary>
    /// <typeparam name="T">The declared entity type whose public readable instance fields and properties are compared.</typeparam>
    /// <param name="excludedNames">
    /// The complete exclusion list, replacing the defaults; null uses the defaults. An empty collection excludes
    /// no members by name. Members marked with <see cref="EntityDataEqualityIgnoreAttribute"/> are always excluded.
    /// Names are matched case-sensitively and unknown names are ignored.
    /// </param>
    /// <returns>A comparer with the behavior described by <see cref="EntityDataEqualityComparer{T}"/>.</returns>
    public static IEqualityComparer<T> Create<T>(IReadOnlyCollection<string>? excludedNames = null)
    {
        return EntityDataEqualityComparer<T>.Create(excludedNames);
    }
}

/// <summary>Creates shallow entity data comparers for deciding whether an entity needs updating.</summary>
/// <typeparam name="T">The declared entity type whose members are compared.</typeparam>
/// <remarks>
/// Compares public readable instance fields and non-indexer properties of T, including inherited members.
/// Members declared only by a runtime subtype are not inspected. Distinct runtime types are unequal.
/// Each included member uses its default equality comparer; collections and navigation properties are not
/// compared recursively. Ignore such members or construct a separate comparer when different rules are required.
/// Null entities compare equal only to null; identical references compare equal. Hash codes use the same included
/// members, so do not change those values while an entity is stored in a hash-based collection using this comparer.
/// </remarks>
public static class EntityDataEqualityComparer<T>
{
    /// <summary>Gets the cached comparer built with the default exclusions when this generic type is initialized.</summary>
    /// <remarks>The default exclusions are fixed; use Create with a custom list for a different policy.</remarks>
    public static readonly IEqualityComparer<T> Instance = CreateCore();

    private static IEqualityComparer<T> CreateCore(IReadOnlyCollection<string>? excludedNames = null)
    {
        var excluded = excludedNames ?? EntityDataEqualityComparer.DefaultExcludedNames;

        var exclusion = typeof(T).GetDataMembers(DataMemberFlags.PublicReadableInstance)
            .Where(m => excluded.Contains(m.Name) || m.IsDefined<EntityDataEqualityIgnoreAttribute>())
            .Select(m => m.MemberInfo);

        var builder = MemberEqualityComparerBuilder
            .Create<T>()
            .AddAllPublicDataMembers(exclusion);

        return builder.Build();
    }

    /// <summary>Creates a comparer using a complete exclusion list, or returns the cached default comparer.</summary>
    /// <param name="excludedNames">
    /// The complete, case-sensitive set of excluded member names, replacing the defaults; null uses the cached
    /// default comparer. An empty collection excludes no members by name. Unknown names are ignored.
    /// <see cref="EntityDataEqualityIgnoreAttribute"/> exclusions apply regardless of this list.
    /// </param>
    /// <returns>
    /// <see cref="Instance"/> when the list is null or set-equal to the current default names; otherwise, a new
    /// comparer. Member selection is fixed at creation and does not track later changes to the supplied collection.
    /// </returns>
    public static IEqualityComparer<T> Create(IReadOnlyCollection<string>? excludedNames = null)
    {
        return excludedNames is null || EntityDataEqualityComparer.DefaultExcludedNames.SetEquals(excludedNames)
            ? Instance
            : CreateCore(excludedNames);
    }
}
