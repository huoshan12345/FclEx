namespace FclEx.EfCore;

public static class EntityEntryExtensions
{
    /// <summary>Copies the source entry's current primary-key values to another entity's CLR members.</summary>
    /// <typeparam name="T">The declared entity type.</typeparam>
    /// <param name="entry">The source entry whose primary-key CurrentValue values are read.</param>
    /// <param name="entity">The target entity whose key members are overwritten. Non-key members are unchanged.</param>
    /// <param name="transform">
    /// An optional function receiving each primary-key property's metadata and current entry value and returning
    /// the target value. Null copies values unchanged. Returned values must be assignable to the target key member.
    /// </param>
    /// <returns>The source entry, for chaining.</returns>
    /// <exception cref="InvalidOperationException">The entry's entity type has no primary key.</exception>
    /// <remarks>
    /// Copies every primary-key component in metadata order, including EF temporary values; alternate keys are
    /// not copied. Shadow key values are read and passed to the transformation, but no value can be written for
    /// a shadow property because the target has no CLR member. Unlike the plain-object overloads, this method
    /// does not reject shadow keys. Does not attach the target, copy IsTemporary flags, execute SQL, or save changes.
    /// Prefer an untracked target; changing a tracked key can invalidate tracking and be rejected during change detection.
    /// Assignments are sequential and are not rolled back if a later transformation or member access fails.
    /// </remarks>
    public static EntityEntry<T> ApplyKeyTo<T>(this EntityEntry<T> entry, T entity, Func<IProperty, object?, object?>? transform = null) where T : class
    {
        var key = entry.Metadata.FindPrimaryKey()
            ?? throw new InvalidOperationException($"{typeof(T).Name} does not have a primary key defined");

        transform ??= (property, value) => value;
        foreach (var property in key.Properties)
        {
            var value = entry.Property(property.Name).CurrentValue;
            var newValue = transform(property, value);
            property.SetValue(entity, newValue);
        }

        return entry;
    }

    /// <summary>Resets another entity's CLR primary-key members to their CLR type defaults using the source entry's mapping.</summary>
    /// <typeparam name="T">The declared entity type.</typeparam>
    /// <param name="entry">The source entry identifying the primary-key members to reset.</param>
    /// <param name="entity">The target entity whose key members are reset. Non-key members are unchanged.</param>
    /// <returns>The source entry, for chaining.</returns>
    /// <exception cref="InvalidOperationException">The entry's entity type has no primary key.</exception>
    /// <remarks>
    /// Uses default values such as zero for integer keys and null for reference-type keys, regardless of the source
    /// values. This does not request database-generated values or save changes. Shadow keys have no target CLR
    /// member and are not written. The source entry is unchanged unless the target is its own entity.
    /// Prefer an untracked target; resetting a tracked key can invalidate tracking. Assignments are not atomic.
    /// </remarks>
    public static EntityEntry<T> ApplyKeyToDefault<T>(this EntityEntry<T> entry, T entity) where T : class
    {
        return entry.ApplyKeyTo(entity, (property, value) => property.ClrType.DefaultValue());
    }

    public static void SetKeyUnmodified<T>(this EntityEntry<T> entry) where T : class
    {
        var key = entry.Metadata.FindPrimaryKey()
            ?? throw new InvalidOperationException($"{typeof(T).Name} does not have a primary key defined");

        foreach (var property in key.Properties)
        {
            var propertyEntry = entry.Property(property);
            propertyEntry.IsModified = false;
            propertyEntry.IsTemporary = false;
        }
    }
}
