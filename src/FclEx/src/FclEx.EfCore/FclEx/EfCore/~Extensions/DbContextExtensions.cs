namespace FclEx.EfCore;

public static partial class DbContextExtensions
{
    public static Task<T?> GetAsync<T, TKey>(this DbSet<T> set, TKey id, bool noTracking = true, CancellationToken cancellationToken = default)
        where T : class, IHasId<TKey>
    {
        var query = noTracking
            ? set.AsNoTracking()
            : set;

        var filter = QueryableHelper.BuildIdFilter<T, TKey>(id);
        return query.FirstOrDefaultAsync(filter, cancellationToken);
    }

    public static async Task<T> GetOrAddAsync<T>(this DbContext context, Expression<Func<T, bool>> filter, Func<T> factory) where T : class
    {
        var entity = await context.Set<T>().FirstOrDefaultAsync(filter);
        if (entity is null)
        {
            entity = factory();
            context.Add(entity);
            await context.SaveChangesAsync();
        }
        return entity;
    }

    public static async Task<T> SaveAsync<T, TKey>(this DbContext context, T entity, params string[] excludeOnUpdate) 
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

            foreach (var name in excludeOnUpdate)
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
        }

        await context.SaveChangesAsync();
        return entity;
    }
    
    public static Task<T> SaveAsync<T>(this DbContext context, T entity, params string[] excludeOnUpdate) 
        where T : class, IHasId<long>
    {
        return context.SaveAsync<T, long>(entity, excludeOnUpdate);
    }

    public static Task<int> InsertAsync<T>(this DbContext context, T item) where T : class
    {
        context.Set<T>().Add(item);
        return context.SaveChangesAsync();
    }

    public static Task<int> InsertRangeAsync<T>(this DbContext context, IEnumerable<T> enumerable) where T : class
    {
        context.Set<T>().AddRange(enumerable);
        return context.SaveChangesAsync();
    }

    public static Task<int> UpdateAsync<T>(this DbContext context, T item) where T : class
    {
        context.Set<T>().Update(item);
        return context.SaveChangesAsync();
    }

    public static Task<int> UpdateRangeAsync<T>(this DbContext context, IEnumerable<T> enumerable) where T : class
    {
        context.Set<T>().UpdateRange(enumerable);
        return context.SaveChangesAsync();
    }

    public static Task<int> DeleteAsync<T>(this DbContext context, T item) where T : class
    {
        context.Set<T>().Remove(item);
        return context.SaveChangesAsync();
    }

    public static Task<int> DeleteRangeAsync<T>(this DbContext context, IEnumerable<T> enumerable) where T : class
    {
        context.Set<T>().RemoveRange(enumerable);
        return context.SaveChangesAsync();
    }

    public static void PartitionChanges<TEntity, TKey>(this DbContext context,
        IEnumerable<TEntity> newItems, IEnumerable<TEntity> existingItems, Func<TEntity, TKey> keySelector,
        Func<TEntity, TEntity> insertMapper, Func<TEntity, TEntity, TEntity>? updateMapper = null) where TKey : notnull where TEntity : class
    {
        context.PartitionChanges(newItems, keySelector, existingItems, keySelector, insertMapper, updateMapper);
    }

    public static void PartitionChanges<TEntity, TDto, TKey>(this DbContext context,
        IEnumerable<TDto> newItems, Func<TDto, TKey> dtoKeySelector,
        IEnumerable<TEntity> existingItems, Func<TEntity, TKey> entityKeySelector,
        Func<TDto, TEntity> insertMapper, Func<TDto, TEntity, TEntity>? updateMapper = null) where TKey : notnull where TEntity : class
    {
        var set = context.Set<TEntity>();
        var existingDic = existingItems.GroupBy(entityKeySelector).ToDictionary(m => m.Key, m => m.First());

        foreach (var dto in newItems)
        {
            var id = dtoKeySelector(dto);
            if (existingDic.TryGetValue(id, out var exist))
            {
                var update = false;
                TEntity entity;
                if (updateMapper is null)
                {
                    entity = exist;
                }
                else
                {
                    entity = updateMapper(dto, exist);
                    update = true;
                }

                if (entity is ISoftDeletable { IsDeleted: true } deletable)
                {
                    deletable.IsDeleted = false;
                    update = true;
                }

                if (update)
                    set.Update(entity);

                existingDic.Remove(id);
            }
            else
            {
                set.Add(insertMapper(dto));
            }
        }

        foreach (var (_, entity) in existingDic)
        {
            set.Remove(entity);
        }
    }
    
    public static Task<int> SoftDeleteAsync<T, TKey>(this DbSet<T> set, TKey id, CancellationToken cancellationToken = default) 
        where T : class, IHasId<TKey>
    {
        var filter = QueryableHelper.BuildIdFilter<T, TKey>(id);
        return set.Where(filter).ExecuteSoftDeleteAsync(cancellationToken);
    }

}