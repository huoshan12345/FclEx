namespace FclEx.EfCore;

public class ModelCacheKeyWithSchemaFactory : IModelCacheKeyFactory
{
    // ReSharper disable once SuspiciousTypeConversion.Global
    public object Create(DbContext context, bool designTime) => new ModelCacheKeyWithSchema(context, (context as IHasSchema)?.Schema, designTime);
}