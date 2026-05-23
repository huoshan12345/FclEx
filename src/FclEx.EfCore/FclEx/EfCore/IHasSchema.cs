namespace FclEx.EfCore;

/// <summary>
/// This is required to be compatible with <see cref="IModelCacheKeyFactory.Create(DbContext, bool)"/>
/// </summary>
public interface IHasSchema
{
    string? Schema { get; }
}