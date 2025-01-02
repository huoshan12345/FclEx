namespace FclEx.EfCore;

partial class DbContextExtensions
{
    public static readonly MethodInfo MethodOfTestEntity = typeof(DbContextExtensions)
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
        var disposable = types.Select(m => MethodOfTestEntity.MakeGenericMethod(m).Invoke(null, [context]))
            .Cast<IDisposable>()
            .Composite();

        using (disposable)
        {
            await context.SaveChangesAsync();
        }
        await context.SaveChangesAsync();
    }
}
