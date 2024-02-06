namespace FclEx.EfCore;

public static class DbContextExtensions
{
    public static async Task<T> GetOrAdd<T>(this DbContext context, Expression<Func<T, bool>> filter, T itemToAdd) where T : class
    {
        var e = await context.Set<T>().Where(filter).FirstOrDefaultAsync();
        if (e == null)
        {
            context.Set<T>().Add(itemToAdd);
            await context.SaveChangesAsync();
            e = itemToAdd;
        }
        return e;
    }

    public static ValueTask<T?> GetAsync<T>(this DbContext context, int key, CancellationToken token) where T : class
    {
        return context.FindAsync<T>([key], token);
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
}