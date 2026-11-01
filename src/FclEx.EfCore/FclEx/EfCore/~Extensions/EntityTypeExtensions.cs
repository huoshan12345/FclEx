namespace FclEx.EfCore;

public static class EntityTypeExtensions
{
    /// <summary>Copies primary-key values from a source entity to a target entity using this entity type's mapping.</summary>
    /// <typeparam name="T">The exact CLR type represented by <paramref name="entityType"/>.</typeparam>
    /// <param name="entityType">The EF metadata defining the primary-key properties to copy.</param>
    /// <param name="source">The entity whose CLR key values are read. It need not be tracked.</param>
    /// <param name="target">The entity whose CLR key members are overwritten. Non-key members are unchanged.</param>
    /// <param name="transform">
    /// An optional function receiving each key property's metadata and source value, and returning the value to
    /// assign to the target. Null copies values unchanged. Returned values must be assignable to the key member.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// The metadata CLR type does not exactly equal T, no primary key is defined, or a key property is a shadow property.
    /// </exception>
    /// <remarks>
    /// Copies all components of a composite primary key in metadata order. Alternate keys are not copied.
    /// Reads and writes CLR properties or fields directly; EF temporary and shadow state is not used.
    /// Does not attach entities, update tracking state, execute SQL, or save changes. Prefer an untracked target:
    /// changing a tracked primary key can invalidate EF tracking and may be rejected during change detection.
    /// This operation is not atomic: if a transformation or member access fails, earlier assignments remain applied.
    /// </remarks>
    public static void ApplyKeyTo<T>(this IEntityType entityType, T source, T target, Func<IProperty, object?, object?>? transform = null) where T : class
    {
        if (entityType.ClrType != typeof(T))
            throw new InvalidOperationException($"EntityType CLR type {entityType.ClrType.Name} does not match {typeof(T).Name}");

        var key = entityType.FindPrimaryKey()
            ?? throw new InvalidOperationException($"{typeof(T).Name} does not have a primary key defined");

        transform ??= (_, value) => value;
        foreach (var property in key.Properties)
        {
            if (property.IsShadowProperty())
                throw new InvalidOperationException($"Cannot apply key: '{property.Name}' on {typeof(T).Name} is a shadow property " +
                                                    $"and has no CLR backing member to read from or write to on a plain object instance.");

            var value = property.GetValue(source);
            var newValue = transform(property, value);
            property.SetValue(target, newValue);
        }
    }
}
