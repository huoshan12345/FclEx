namespace FclEx.EfCore;

partial class DbContextExtensions
{
    private static readonly MethodInfo _methodTestEntity = typeof(DbContextExtensions)
        .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        .Single(m => m.Name == nameof(TestEntity));

    /// <summary>
    /// Adds a default-constructed entity to the context and returns a handle that removes it when disposed.
    /// </summary>
    /// <typeparam name="T">The entity type, which must have a public parameterless constructor.</typeparam>
    /// <param name="context">The context whose set receives the entity.</param>
    /// <returns>A disposable handle that marks the entity for removal.</returns>
    /// <remarks>This method does not call <see cref="DbContext.SaveChanges()"/>.</remarks>
    public static IDisposable TestEntity<T>(this DbContext context) where T : class, new()
    {
        var e = new T();
        context.Set<T>().Add(e);
        return Disposable.Create(() => context.Set<T>().Remove(e));
    }

    /// <summary>
    /// Verifies that default instances of the supplied entity types can be inserted and then deleted.
    /// </summary>
    /// <param name="context">The context used for both save operations.</param>
    /// <param name="types">Entity CLR types with public parameterless constructors.</param>
    /// <returns>A task that completes after the inserted entities have been removed and both changes have been saved.</returns>
    public static async Task TestEntities(this DbContext context, params Type[] types)
    {
        await context.TestEntities(default, types);
    }

    /// <summary>
    /// Verifies that default instances of the supplied entity types can be inserted and then deleted.
    /// </summary>
    /// <param name="context">The context used for both save operations.</param>
    /// <param name="cancellationToken">A token observed by both save operations.</param>
    /// <param name="types">Entity CLR types with public parameterless constructors.</param>
    /// <returns>A task that completes after the inserted entities have been removed and both changes have been saved.</returns>
    public static async Task TestEntities(this DbContext context, CancellationToken cancellationToken, params Type[] types)
    {
        var disposable = types.Select(m => _methodTestEntity.MakeGenericMethod(m).Invoke(null, [context]))
            .Cast<IDisposable>()
            .Merge();

        using (disposable)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        await context.SaveChangesAsync(cancellationToken);
    }
}
