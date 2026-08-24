namespace FclEx.Dapper;

/// <summary>
/// Selects the entity types for which FclEx should explicitly install Dapper column mappings.
/// </summary>
public sealed class FclExDapperConfigurationBuilder
{
    private readonly HashSet<Type> _entityTypes = new();
    private IEntityMappingSource _mappingSource = DapperHelper.DefaultEntityMappingSource;

    /// <summary>
    /// Selects the mapping source used to build the registered Dapper type maps.
    /// </summary>
    /// <param name="mappingSource">The source that owns the mappings for all selected entity types.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mappingSource"/> is <see langword="null"/>.</exception>
    public FclExDapperConfigurationBuilder UseEntityMappingSource(IEntityMappingSource mappingSource)
    {
        _mappingSource = mappingSource ?? throw new ArgumentNullException(nameof(mappingSource));
        return this;
    }

    /// <summary>
    /// Adds the mapping for one entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type whose selected-source mapping should be used by Dapper.</typeparam>
    /// <returns>This builder.</returns>
    public FclExDapperConfigurationBuilder AddColumnMapping<TEntity>()
    {
        return AddColumnMappings(typeof(TEntity));
    }

    /// <summary>
    /// Adds mappings for the specified entity types.
    /// </summary>
    /// <param name="entityTypes">Entity types whose selected-source mappings should be used by Dapper.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="entityTypes"/> or one of its elements is <see langword="null"/>.
    /// </exception>
    public FclExDapperConfigurationBuilder AddColumnMappings(params Type[] entityTypes)
    {
        if (entityTypes is null)
            throw new ArgumentNullException(nameof(entityTypes));

        foreach (var entityType in entityTypes)
        {
            if (entityType is null)
                throw new ArgumentNullException(nameof(entityTypes), "An entity type cannot be null.");

            _entityTypes.Add(entityType);
        }

        return this;
    }

    /// <summary>
    /// Adds exported types from an assembly that declare <see cref="TableAttribute"/> or contain a
    /// property declaring <see cref="ColumnAttribute"/>.
    /// </summary>
    /// <param name="assembly">The assembly to inspect immediately.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// Assembly inspection is opt-in and limited to the supplied assembly. FclEx never scans the current
    /// <see cref="AppDomain"/> automatically.
    /// </remarks>
    public FclExDapperConfigurationBuilder AddColumnMappingsFromAssembly(Assembly assembly)
    {
        if (assembly is null)
            throw new ArgumentNullException(nameof(assembly));

        var entityTypes = assembly.ExportedTypes
            .Where(type => type.GetCustomAttribute<TableAttribute>() is not null
                           || type.GetProperties().Any(property => property.GetCustomAttribute<ColumnAttribute>() is not null))
            .ToArray();

        return AddColumnMappings(entityTypes);
    }

    /// <summary>
    /// Applies the selected mappings to Dapper's process-wide type-map registry.
    /// </summary>
    /// <param name="conflictBehavior">How to handle a custom type map already owned by another component.</param>
    /// <returns>
    /// A registration that keeps the applied mappings active. Dispose it to release this configuration and,
    /// when this is the last equivalent registration, restore the type maps that preceded it.
    /// </returns>
    /// <remarks>
    /// Mapping construction completes before Dapper is modified. The selected <see cref="IEntityMappingSource"/>
    /// must return stable mapping instances as required by its contract.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// An existing custom type map conflicts with the configuration and <paramref name="conflictBehavior"/>
    /// is <see cref="DapperRegistrationConflictBehavior.Throw"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="conflictBehavior"/> is invalid.</exception>
    public FclExDapperRegistration Apply(
        DapperRegistrationConflictBehavior conflictBehavior = DapperRegistrationConflictBehavior.Throw)
    {
        return DapperHelper.ApplyColumnMappings(_entityTypes, _mappingSource, conflictBehavior);
    }
}
