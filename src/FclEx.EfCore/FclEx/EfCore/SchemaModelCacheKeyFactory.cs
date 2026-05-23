namespace FclEx.EfCore;

public class SchemaModelCacheKeyFactory : IModelCacheKeyFactory
{
    // ReSharper disable once SuspiciousTypeConversion.Global
    public object Create(DbContext context, bool designTime) => new SchemaModelCacheKey(context, (context as IHasSchema)?.Schema, designTime);
}