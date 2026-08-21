namespace FclEx.EfCore;

/// <summary>
/// Provides common persistence, synchronization, and testing operations for <see cref="DbContext"/>.
/// </summary>
public static partial class DbContextExtensions
{
    /// <summary>
    /// Retrieves an entity matching the specified filter from the database.
    /// If no matching entity is found, creates a new entity using the provided factory function, adds it to the database, and saves changes.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="context">The <see cref="DbContext"/> instance used for database operations.</param>
    /// <param name="filter">The filter expression to locate an existing entity.</param>
    /// <param name="factory">The factory function to create a new entity if no match is found.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>The existing or newly added entity.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="context"/>, <paramref name="filter"/>, or <paramref name="factory"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This operation is not atomic. Concurrent callers can both observe that no entity exists and then attempt
    /// to insert one. Use a database unique constraint and handle the resulting conflict when uniqueness matters.
    /// </remarks>
    public static async Task<T> GetOrAddAsync<T>(this DbContext context, Expression<Func<T, bool>> filter, Func<T> factory,
        CancellationToken cancellationToken = default) where T : class
    {
        Check.NotNull(context);
        Check.NotNull(filter);
        Check.NotNull(factory);

        var entity = await context.Set<T>().FirstOrDefaultAsync(filter, cancellationToken);
        if (entity is null)
        {
            entity = factory();
            context.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
        }
        return entity;
    }

    /// <summary>
    /// Saves an entity to the database by determining whether it should be added or updated.
    /// If the entity's ID is the default value (e.g., 0 for integers or Guid.Empty for GUIDs),
    /// it is treated as a new entity and marked for insertion. Otherwise, it is treated as
    /// an existing entity and marked for update.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's ID.</typeparam>
    /// <param name="context">The <see cref="DbContext"/> instance used for database operations.</param>
    /// <param name="entity">The entity to be saved.</param>
    /// <param name="excludeOnUpdate">
    /// Names of properties or navigation properties that should not be modified during an update operation.
    /// </param>
    /// <returns>The saved entity after changes have been persisted to the database.</returns>
    /// <remarks>
    /// This method is designed for entities with auto-generated IDs. It checks the ID value
    /// to determine whether the entity is new or existing. For new entities, the state is set
    /// to <see cref="EntityState.Added"/>. For existing entities, the state is set to 
    /// <see cref="EntityState.Modified"/>, and any properties or navigation properties specified 
    /// in <paramref name="excludeOnUpdate"/> are excluded from modification.
    /// </remarks>
    public static async Task<T> SaveAsync<T, TKey>(this DbContext context, T entity, params IEnumerable<string> excludeOnUpdate)
        where T : class, IHasId<TKey>
    {
        return await context.SaveAsync<T, TKey>(entity, default, excludeOnUpdate);
    }

    /// <summary>
    /// Inserts an entity whose key is the default value, or updates an entity whose key is non-default.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TKey">The entity key type.</typeparam>
    /// <param name="context">The context used to save the entity.</param>
    /// <param name="entity">The entity to insert or update.</param>
    /// <param name="cancellationToken">A token to observe while saving changes.</param>
    /// <param name="excludeOnUpdate">Property or navigation names that should not be updated for an existing entity.</param>
    /// <returns>The supplied entity after changes have been saved.</returns>
    public static async Task<T> SaveAsync<T, TKey>(this DbContext context, T entity, CancellationToken cancellationToken,
        params IEnumerable<string> excludeOnUpdate)
        where T : class, IHasId<TKey>
    {
        var entry = context.Entry(entity);

        if (EqualityComparer<TKey>.Default.Equals(entity.Id, default))
        {
            entry.State = EntityState.Added;
        }
        else
        {
            entry.State = EntityState.Modified;
            entry.ExcludeFromUpdate(excludeOnUpdate);
        }

        await context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <summary>
    /// Prevents the specified properties or navigation members from being marked as modified
    /// during an update operation.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entry">The <see cref="EntityEntry{T}"/> representing the tracked entity.</param>
    /// <param name="propertyNames">A collection of property or navigation names to exclude from updates.</param>
    /// <returns>The same <see cref="EntityEntry{T}"/> instance for chaining.</returns>
    /// <remarks>
    /// This method sets <see cref="PropertyEntry.IsModified"/> or <see cref="NavigationEntry.IsModified"/>
    /// to <see langword="false"/> for each specified member, ensuring they are not included in database update operations.
    /// </remarks>
    public static EntityEntry<T> ExcludeFromUpdate<T>(this EntityEntry<T> entry, params IEnumerable<string> propertyNames) where T : class
    {
        foreach (var name in propertyNames)
        {
            if (entry.Metadata.FindProperty(name) is { } property)
            {
                entry.Property(property).IsModified = false;
            }
            else if (entry.Metadata.FindNavigation(name) is { } navigation)
            {
                entry.Navigation(navigation).IsModified = false;
            }
        }
        return entry;
    }

    /// <summary>
    /// Inserts or updates an entity with a <see cref="long"/> key, based on whether its key is the default value.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="context">The context used to save the entity.</param>
    /// <param name="entity">The entity to insert or update.</param>
    /// <param name="excludeOnUpdate">Property or navigation names that should not be updated for an existing entity.</param>
    /// <returns>The supplied entity after changes have been saved.</returns>
    public static Task<T> SaveAsync<T>(this DbContext context, T entity, params IEnumerable<string> excludeOnUpdate)
        where T : class, IHasId<long>
    {
        return context.SaveAsync<T, long>(entity, excludeOnUpdate);
    }

    /// <summary>
    /// Inserts or updates an entity with a <see cref="long"/> key, based on whether its key is the default value.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="context">The context used to save the entity.</param>
    /// <param name="entity">The entity to insert or update.</param>
    /// <param name="cancellationToken">A token to observe while saving changes.</param>
    /// <param name="excludeOnUpdate">Property or navigation names that should not be updated for an existing entity.</param>
    /// <returns>The supplied entity after changes have been saved.</returns>
    public static Task<T> SaveAsync<T>(this DbContext context, T entity, CancellationToken cancellationToken,
        params IEnumerable<string> excludeOnUpdate)
        where T : class, IHasId<long>
    {
        return context.SaveAsync<T, long>(entity, cancellationToken, excludeOnUpdate);
    }

    /// <summary>
    /// Adds an entity and immediately saves the context.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="context">The context used for the operation.</param>
    /// <param name="item">The entity to add.</param>
    /// <param name="cancellationToken">A token to observe while saving changes.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public static Task<int> InsertAsync<T>(this DbContext context, T item, CancellationToken cancellationToken = default) where T : class
    {
        context.Set<T>().Add(item);
        return context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a sequence of entities and immediately saves the context.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="context">The context used for the operation.</param>
    /// <param name="enumerable">The entities to add.</param>
    /// <param name="cancellationToken">A token to observe while saving changes.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public static Task<int> InsertRangeAsync<T>(this DbContext context, IEnumerable<T> enumerable, CancellationToken cancellationToken = default) where T : class
    {
        context.Set<T>().AddRange(enumerable);
        return context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Marks an entity as modified and immediately saves the context.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="context">The context used for the operation.</param>
    /// <param name="item">The entity to update.</param>
    /// <param name="cancellationToken">A token to observe while saving changes.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public static Task<int> UpdateAsync<T>(this DbContext context, T item, CancellationToken cancellationToken = default) where T : class
    {
        context.Set<T>().Update(item);
        return context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Marks a sequence of entities as modified and immediately saves the context.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="context">The context used for the operation.</param>
    /// <param name="enumerable">The entities to update.</param>
    /// <param name="cancellationToken">A token to observe while saving changes.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public static Task<int> UpdateRangeAsync<T>(this DbContext context, IEnumerable<T> enumerable, CancellationToken cancellationToken = default) where T : class
    {
        context.Set<T>().UpdateRange(enumerable);
        return context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Marks an entity for deletion and immediately saves the context.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="context">The context used for the operation.</param>
    /// <param name="item">The entity to delete.</param>
    /// <param name="cancellationToken">A token to observe while saving changes.</param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <remarks>A context that applies soft-delete state rules may convert this operation into an update.</remarks>
    public static Task<int> DeleteAsync<T>(this DbContext context, T item, CancellationToken cancellationToken = default) where T : class
    {
        context.Set<T>().Remove(item);
        return context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Marks a sequence of entities for deletion and immediately saves the context.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="context">The context used for the operation.</param>
    /// <param name="enumerable">The entities to delete.</param>
    /// <param name="cancellationToken">A token to observe while saving changes.</param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <remarks>A context that applies soft-delete state rules may convert deletions into updates.</remarks>
    public static Task<int> DeleteRangeAsync<T>(this DbContext context, IEnumerable<T> enumerable, CancellationToken cancellationToken = default) where T : class
    {
        context.Set<T>().RemoveRange(enumerable);
        return context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes the entity with the supplied key, using soft deletion when the entity type supports it.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TKey">The entity key type.</typeparam>
    /// <param name="set">The entity set to query.</param>
    /// <param name="id">The key of the entity to delete.</param>
    /// <param name="cancellationToken">A token to observe while executing the database command.</param>
    /// <returns>The number of rows affected.</returns>
    /// <remarks>The operation executes directly in the database and does not synchronize tracked instances.</remarks>
    public static Task<int> SoftDeleteAsync<T, TKey>(this DbSet<T> set, TKey id, CancellationToken cancellationToken = default)
        where T : class, IHasId<TKey>
    {
        var filter = QueryableHelper.BuildIdFilter<T, TKey>(id);
        return set.Where(filter).ExecuteSoftDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the <see cref="EntityEntry{T}"/> for the given entity by matching its primary key
    /// against already-tracked entries in the ChangeTracker, instead of relying on reference equality
    /// like <see cref="DbContext.Entry{T}"/> does.
    /// If an entry with the same primary key value(s) is already being tracked, that existing entry is
    /// returned. Otherwise, <see langword="null"/> is returned, indicating that no tracked entry exists for the given entity instance.
    /// This helps avoid the "another instance with the same key value is already being tracked" exception
    /// that occurs when two different object instances with the same key are both attached/tracked.
    /// Supports composite primary keys and shadow key properties.
    /// Note: this only searches entries already in the ChangeTracker; it does not query the database
    /// (unlike <see cref="DbSet{T}.Find"/>).
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="entity">The entity instance whose primary key value(s) will be used for lookup.</param>
    /// <returns>
    /// The existing tracked <see cref="EntityEntry{T}"/> matching the entity's primary key,
    /// or <see langword="null"/> if no tracked entry exists for the given entity instance.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <typeparamref name="T"/> is not an entity type on this context, or has no primary key defined.
    /// </exception>
    public static EntityEntry<T>? GetEntry<T>(this DbContext context, T entity) where T : class
    {
        var entityType = context.Model.FindEntityType(typeof(T))
            ?? throw new InvalidOperationException($"{typeof(T).Name} is not an entity type on this DbContext");

        var key = entityType.FindPrimaryKey()
            ?? throw new InvalidOperationException($"{typeof(T).Name} does not have a primary key defined");

        // Read the current primary key value(s) from the given entity (supports composite keys)
        var keyProperties = key.Properties;
        var keyValues = new object?[keyProperties.Count];
        for (int i = 0; i < keyProperties.Count; i++)
        {
            keyValues[i] = keyProperties[i].GetValue(entity);
        }

        // Look for an already-tracked entry with the same key value(s)
        var existing = context.ChangeTracker.Entries<T>()
            .FirstOrDefault(e =>
            {
                if (ReferenceEquals(e.Entity, entity))
                    return true;

                // ReSharper disable once LoopCanBeConvertedToQuery
                for (int i = 0; i < keyProperties.Count; i++)
                {
                    var value = e.Property(keyProperties[i].Name).CurrentValue;
                    if (Equals(value, keyValues[i]) == false)
                        return false;
                }
                return true;
            });

        return existing;
    }

    /// <summary>
    /// Gets the <see cref="EntityEntry{T}"/> for the given entity by matching its primary key
    /// against already-tracked entries in the ChangeTracker, instead of relying on reference equality
    /// like <see cref="DbContext.Entry{T}"/> does.
    /// If an entry with the same primary key value(s) is already being tracked, that existing entry is
    /// returned. Otherwise, a new entry is created and returned for the given entity instance
    /// (equivalent to calling <c>context.Entry(entity)</c>).
    /// This helps avoid the "another instance with the same key value is already being tracked" exception
    /// that occurs when two different object instances with the same key are both attached/tracked.
    /// Supports composite primary keys and shadow key properties.
    /// Note: this only searches entries already in the ChangeTracker; it does not query the database
    /// (unlike <see cref="DbSet{T}.Find"/>).
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="entity">The entity instance whose primary key value(s) will be used for lookup.</param>
    /// <returns>
    /// The existing tracked <see cref="EntityEntry{T}"/> matching the entity's primary key,
    /// or a new <see cref="EntityEntry{T}"/> for the given entity if none is currently tracked.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <typeparamref name="T"/> is not an entity type on this context, or has no primary key defined.
    /// </exception>
    public static EntityEntry<T> GetOrCreateEntry<T>(this DbContext context, T entity) where T : class
    {
        return context.GetEntry(entity) ?? context.Entry(entity);
    }

    /// <summary>
    /// Gets the <see cref="EntityEntry{T}"/> for the given entity by matching its primary key
    /// against already-tracked entries in the ChangeTracker, instead of relying on reference equality
    /// like <see cref="DbContext.Entry{T}"/> does.
    /// If an entry with the same primary key value(s) is already being tracked, that existing entry is
    /// returned. Otherwise, a new entry is created and returned for the given entity instance
    /// (equivalent to calling <c>context.Entry(entity)</c>).
    /// This helps avoid the "another instance with the same key value is already being tracked" exception
    /// that occurs when two different object instances with the same key are both attached/tracked.
    /// Supports composite primary keys and shadow key properties.
    /// Note: this only searches entries already in the ChangeTracker; it does not query the database
    /// (unlike <see cref="DbSet{T}.Find"/>).
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="entity">The entity instance whose primary key value(s) will be used for lookup.</param>
    /// <returns>
    /// The existing tracked <see cref="EntityEntry{T}"/> matching the entity's primary key,
    /// or a new <see cref="EntityEntry{T}"/> for the given entity if none is currently tracked.
    /// </returns>
    public static EntityEntry<T> GetOrReplaceEntry<T>(this DbContext context, T entity) where T : class
    {
        var existing = context.GetEntry(entity);

        // ReSharper disable once InvertIf
        if (existing is not null && ReferenceEquals(existing.Entity, entity) == false)
        {
            existing.State = EntityState.Detached;
            existing = null;
        }

        return existing ?? context.Entry(entity);
    }

    public static DbContext ApplyKeyTo<T>(this DbContext context, T source, T target, Func<IProperty, object?, object?>? transform = null) where T : class
    {
        var entityType = context.Model.FindEntityType(typeof(T))
            ?? throw new InvalidOperationException($"{typeof(T).Name} is not an entity type on this DbContext");

        entityType.ApplyKeyTo(source, target, transform);
        return context;
    }

    /// <summary>
    /// Applies the given DTO changes to the specified <see cref="DbContext"/> set.
    /// Determines which entities should be inserted, updated, or deleted,
    /// and returns the results of those operations.
    /// </summary>
    /// <typeparam name="TEntity">The entity type tracked by the DbContext.</typeparam>
    /// <typeparam name="TDto">The DTO type representing incoming data.</typeparam>
    /// <typeparam name="TKey">The key type used to match DTOs and entities.</typeparam>
    /// <param name="context">The database context to operate on.</param>
    /// <param name="dtos">The new DTOs representing desired data state.</param>
    /// <param name="dtoKey">Function to extract the key from a DTO.</param>
    /// <param name="existingEntities">The existing entities retrieved from the database.</param>
    /// <param name="entityKey">Function to extract the key from an entity.</param>
    /// <param name="insertEntity">Function to map a DTO to a new entity instance for insertion.</param>
    /// <param name="updateEntity">
    /// Optional function that maps a DTO and an existing entity to an updated entity.
    /// If not provided or returns null, the entity remains unchanged.
    /// </param>
    /// <param name="allowDeletion">
    /// When <see langword="true"/>, any entities that exist in the database but not in the DTOs will be deleted.
    /// Set to <see langword="false"/> when performing partial updates where deletion should not occur.
    /// </param>
    /// <param name="excludeOnUpdate">
    /// Names of properties or navigation properties that should not be modified during an update operation.
    /// </param>
    /// <returns>
    /// An <see cref="EntityChanges{TEntity}"/> object containing
    /// all inserted, updated, and deleted entities.
    /// </returns>
    public static EntityChanges<TEntity> ApplyChanges<TEntity, TDto, TKey>(
        this DbContext context,
        IEnumerable<TDto> dtos,
        Func<TDto, TKey> dtoKey,
        IEnumerable<TEntity> existingEntities,
        Func<TEntity, TKey> entityKey,
        Func<TDto, TEntity> insertEntity,
        Func<TDto, TEntity, TEntity?>? updateEntity = null,
        bool allowDeletion = false,
        params string[] excludeOnUpdate)
        where TEntity : class
        where TKey : notnull
    {
        var set = context.Set<TEntity>();
        var existingDic = existingEntities.ToMultiValueDictionary(entityKey, m => m);

        var inserted = new List<TEntity>();
        var updated = new List<EntityUpdate<TEntity>>();
        var deleted = new List<TEntity>();
        var existingToUpdate = new HashSet<TEntity>();

        foreach (var dto in dtos)
        {
            var key = dtoKey(dto);
            if (existingDic.TryGetValue(key, out var entities) == false)
            {
                var newEntity = insertEntity(dto);
                var entry = context.GetEntry(newEntity);
                if (entry is null)
                {
                    entry = context.Entry(newEntity);
                }
                else
                {
                    // If the entity is already being tracked, we need to update its values with the new entity's values.
                    entry.CurrentValues.SetValues(newEntity);
                }

                // Ensure the new entity has its key applied to the default values if necessary.
                entry.ApplyKeyToDefault(entry.Entity);
                entry.State = EntityState.Added;

                inserted.Add(newEntity);
                continue;
            }

            var entity = entities.Last();
            var update = false;
            TEntity? updatedEntity;

            if (updateEntity is null)
            {
                updatedEntity = entity;
            }
            else
            {
                updatedEntity = updateEntity(dto, entity);
                update = updatedEntity is not null;
            }

            // recover soft-deleted entity if the updated entity is marked as deleted
            if (updatedEntity is ISoftDeletable { IsDeleted: true } deletable)
            {
                deletable.IsDeleted = false;
                update = true;
            }

            // ReSharper disable once InvertIf
            if (update && updatedEntity is not null)
            {
                context.ApplyKeyTo(entity, updatedEntity); // 更新主键
                var entry = context.GetEntry(updatedEntity);
                entry ??= context.Entry(updatedEntity);
                entry.SetKeyUnmodified();
                entry.State = EntityState.Modified;
                entry.ExcludeFromUpdate(excludeOnUpdate);
                updated.Add(new(updatedEntity, entity));
                existingToUpdate.Add(entity);
            }
        }

        // ReSharper disable once InvertIf
        if (allowDeletion)
        {
            foreach (var (_, entities) in existingDic)
            {
                foreach (var entity in entities)
                {
                    if (existingToUpdate.Contains(entity))
                        continue;

                    set.Remove(entity);
                    deleted.Add(entity);
                }
            }
        }

        return new EntityChanges<TEntity>(inserted, updated, deleted);
    }

    /// <summary>
    /// Applies changes between two sets of entities of the same type, determining
    /// which should be inserted, updated, or deleted, and applies them to the <see cref="DbContext"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type tracked by the DbContext.</typeparam>
    /// <typeparam name="TKey">The key type used to identify entities.</typeparam>
    /// <param name="context">The database context to operate on.</param>
    /// <param name="newEntities">The new set of entities representing the desired state.</param>
    /// <param name="existingEntities">The existing entities retrieved from the database.</param>
    /// <param name="entityKey">Function to extract the key from an entity.</param>
    /// <param name="insertEntity">
    /// Optional function to create a new entity from a source entity.
    /// Defaults to the identity function (<c>e =&gt; e</c>).
    /// </param>
    /// <param name="updateEntity">
    /// Optional function that maps a DTO and an existing entity to an updated entity.
    /// If not provided or returns null, the entity remains unchanged.
    /// </param>
    /// <param name="allowDeletion">
    /// When <see langword="true"/>, any entities that exist in the database but not in <paramref name="newEntities"/> will be deleted.
    /// </param>
    /// <param name="excludeOnUpdate">
    /// Names of properties or navigation properties that should not be modified during an update operation.
    /// </param>
    /// <returns>
    /// An <see cref="EntityChanges{TEntity}"/> describing inserted, updated, and deleted entities.
    /// </returns>
    public static EntityChanges<TEntity> ApplyChanges<TEntity, TKey>(
        this DbContext context,
        IEnumerable<TEntity> newEntities,
        IEnumerable<TEntity> existingEntities,
        Func<TEntity, TKey> entityKey,
        Func<TEntity, TEntity>? insertEntity = null,
        Func<TEntity, TEntity, TEntity?>? updateEntity = null,
        bool allowDeletion = false,
        params string[] excludeOnUpdate)
        where TEntity : class
        where TKey : notnull
    {
        insertEntity ??= e => e;
        updateEntity ??= (e, _) => e;
        return context.ApplyChanges(
            newEntities,
            entityKey,
            existingEntities,
            entityKey,
            insertEntity,
            updateEntity,
            allowDeletion,
            excludeOnUpdate);
    }
}
