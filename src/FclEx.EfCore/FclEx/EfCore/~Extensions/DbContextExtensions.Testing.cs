namespace FclEx.EfCore;

partial class DbContextExtensions
{
    private static readonly MethodInfo _methodTestEntity = typeof(DbContextExtensions)
        .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        .Single(m => m.Name == nameof(TestEntity));

    public static IDisposable TestEntity<T>(this DbContext context) where T : class, new()
    {
        var e = new T();
        context.Set<T>().Add(e);
        return Disposable.Create(() => context.Set<T>().Remove(e));
    }

    public static async Task TestEntities(this DbContext context, params Type[] types)
    {
        var disposable = types.Select(m => _methodTestEntity.MakeGenericMethod(m).Invoke(null, [context]))
            .Cast<IDisposable>()
            .Merge();

        using (disposable)
        {
            await context.SaveChangesAsync();
        }
        await context.SaveChangesAsync();
    }
}
