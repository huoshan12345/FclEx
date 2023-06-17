using System;

namespace FclEx.EfCore;

public class ModelCacheKeyWithSchema : ModelCacheKey, IHasSchema
{
    public string? Schema { get; }
    public bool DesignTime { get; }

    public ModelCacheKeyWithSchema(DbContext context, string? schema, bool designTime) : base(context)
    {
        DesignTime = designTime;
        Schema = schema;
    }

    protected override bool Equals(ModelCacheKey key)
    {
        return key is ModelCacheKeyWithSchema other
               && base.Equals(other)
               && Schema == other.Schema
               && DesignTime == other.DesignTime;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Schema, DesignTime);
    }
}