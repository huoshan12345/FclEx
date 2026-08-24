namespace FclEx.Dapper;

/// <summary>
/// Provides the entity mappings used to generate FclEx.Dapper CRUD SQL and parameters.
/// </summary>
/// <remarks>
/// An implementation must return the same immutable <see cref="EntityMapping"/> instance for repeated
/// requests for the same entity type. This stability allows generated SQL to be cached by mapping identity.
/// </remarks>
public interface IEntityMappingSource
{
    /// <summary>
    /// Gets the mapping for an entity type.
    /// </summary>
    /// <param name="entityType">The CLR entity type.</param>
    /// <returns>The stable mapping for <paramref name="entityType"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entityType"/> is <see langword="null"/>.</exception>
    EntityMapping GetMapping(Type entityType);
}
