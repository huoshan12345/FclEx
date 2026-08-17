namespace FclEx.EfCore;

/// <summary>
/// Creates model cache keys that include the schema exposed by <see cref="IHasSchema"/>.
/// </summary>
public class SchemaModelCacheKeyFactory : IModelCacheKeyFactory
{
    // ReSharper disable once SuspiciousTypeConversion.Global
    /// <inheritdoc />
    public object Create(DbContext context, bool designTime) => new SchemaModelCacheKey(context, (context as IHasSchema)?.Schema, designTime);
}
