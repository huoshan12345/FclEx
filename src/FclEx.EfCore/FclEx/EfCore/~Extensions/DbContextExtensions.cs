using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FclEx.EfCore;

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

    public static Task<T> SaveAsync<T>(this DbContext context, T entity, params IEnumerable<string> excludeOnUpdate)
        where T : class, IHasId<long>
    {
        return context.SaveAsync<T, long>(entity, excludeOnUpdate);
    }

    public static Task<T> SaveAsync<T>(this DbContext context, T entity, CancellationToken cancellationToken,
        params IEnumerable<string> excludeOnUpdate)
        where T : class, IHasId<long>
    {
        return context.SaveAsync<T, long>(entity, cancellationToken, excludeOnUpdate);
    }

    public static Task<int> InsertAsync<T>(this DbContext context, T item, CancellationToken cancellationToken = default) where T : class
    {
        context.Set<T>().Add(item);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task<int> InsertRangeAsync<T>(this DbContext context, IEnumerable<T> enumerable, CancellationToken cancellationToken = default) where T : class
    {
        context.Set<T>().AddRange(enumerable);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task<int> UpdateAsync<T>(this DbContext context, T item, CancellationToken cancellationToken = default) where T : class
    {
        context.Set<T>().Update(item);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task<int> UpdateRangeAsync<T>(this DbContext context, IEnumerable<T> enumerable, CancellationToken cancellationToken = default) where T : class
    {
        context.Set<T>().UpdateRange(enumerable);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task<int> DeleteAsync<T>(this DbContext context, T item, CancellationToken cancellationToken = default) where T : class
    {
        context.Set<T>().Remove(item);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task<int> DeleteRangeAsync<T>(this DbContext context, IEnumerable<T> enumerable, CancellationToken cancellationToken = default) where T : class
    {
        context.Set<T>().RemoveRange(enumerable);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task<int> SoftDeleteAsync<T, TKey>(this DbSet<T> set, TKey id, CancellationToken cancellationToken = default)
        where T : class, IHasId<TKey>
    {
        var filter = QueryableHelper.BuildIdFilter<T, TKey>(id);
        return set.Where(filter).ExecuteSoftDeleteAsync(cancellationToken);
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
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="existingEntities"/> contains duplicate keys, or when
    /// <paramref name="dtos"/> contains duplicate non-default keys. Multiple default DTO keys are allowed
    /// because they commonly represent entities whose keys will be generated by the database.
    /// </exception>
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
        var existingDic = ToDictionary(existingEntities, entityKey, nameof(existingEntities));
        var keyedDtos = ToListWithUniqueKeys(dtos, dtoKey, nameof(dtos));

        var changes = new EntityChanges<TEntity>();
        foreach (var (key, dto) in keyedDtos)
        {
            if (existingDic.TryGetValue(key, out var entity))
            {
                var update = false;
                TEntity? updatedEntity;
                var restoreSoftDeletedEntity = entity is ISoftDeletable { IsDeleted: true };
                var originalDeletedAt = entity is IHasDeletedAt hasDeletedAt
                    ? hasDeletedAt.DeletedAt
                    : default;

                if (updateEntity is null)
                {
                    updatedEntity = entity;
                }
                else
                {
                    updatedEntity = updateEntity(dto, entity);
                    update = updatedEntity is not null;
                }

                if (restoreSoftDeletedEntity && updatedEntity is ISoftDeletable deletable)
                {
                    deletable.IsDeleted = false;
                    if (updatedEntity is IHasDeletedAt updatedHasDeletedAt)
                        updatedHasDeletedAt.DeletedAt = default;
                    update = true;
                }

                if (update && updatedEntity is not null)
                {
                    var entry = context.Entry(entity);

                    if (ReferenceEquals(updatedEntity, entity) == false)
                    {
                        entry.CurrentValues.SetValues(updatedEntity);
                    }

                    entry.State = EntityState.Modified;
                    entry.ExcludeFromUpdate(excludeOnUpdate);

                    if (restoreSoftDeletedEntity)
                    {
                        entry.Property(nameof(ISoftDeletable.IsDeleted)).OriginalValue = true;
                        entry.Property(nameof(ISoftDeletable.IsDeleted)).IsModified = true;

                        if (updatedEntity is IHasDeletedAt)
                        {
                            entry.Property(nameof(IHasDeletedAt.DeletedAt)).OriginalValue = originalDeletedAt;
                            entry.Property(nameof(IHasDeletedAt.DeletedAt)).IsModified = true;
                        }
                    }

                    changes.Updated.Add(new(updatedEntity, entity));
                }

                // // Remove matched entity from deletion candidates since it is present in the incoming DTOs.
                existingDic.Remove(key);
            }
            else
            {
                var newEntity = insertEntity(dto);
                set.Add(newEntity);
                changes.Inserted.Add(newEntity);
            }
        }

        if (allowDeletion)
        {
            foreach (var (_, entity) in existingDic)
            {
                set.Remove(entity);
                changes.Deleted.Add(entity);
            }
        }
        return changes;

        static Dictionary<TKey, TValue> ToDictionary<TValue>(
            IEnumerable<TValue> values,
            Func<TValue, TKey> keySelector,
            string paramName)
        {
            var result = new Dictionary<TKey, TValue>();
            foreach (var value in values)
            {
                if (result.TryAdd(keySelector(value), value) == false)
                    throw new ArgumentException("The collection contains duplicate keys.", paramName);
            }

            return result;
        }

        static List<(TKey Key, TValue Value)> ToListWithUniqueKeys<TValue>(
            IEnumerable<TValue> values,
            Func<TValue, TKey> keySelector,
            string paramName)
        {
            var result = new List<(TKey Key, TValue Value)>();
            var keys = new HashSet<TKey>();
            foreach (var value in values)
            {
                var key = keySelector(value);
                if (EqualityComparer<TKey>.Default.Equals(key, default) == false && keys.Add(key) == false)
                    throw new ArgumentException("The collection contains duplicate keys.", paramName);

                result.Add((key, value));
            }

            return result;
        }
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
        return context.ApplyChanges(
            newEntities,
            entityKey,
            existingEntities,
            entityKey, insertEntity,
            updateEntity,
            allowDeletion,
            excludeOnUpdate);
    }
}
