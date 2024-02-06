using System;

namespace FclEx.EfCore;

public class SchemaModelCacheKey : ModelCacheKey, IHasSchema
{
    public string? Schema { get; }
    public bool DesignTime { get; }

    public SchemaModelCacheKey(DbContext context, string? schema, bool designTime) : base(context)
    {
        DesignTime = designTime;
        Schema = schema;
    }

    protected override bool Equals(ModelCacheKey key)
    {
        return key is SchemaModelCacheKey other
               && base.Equals(other)
               && Schema == other.Schema
               && DesignTime == other.DesignTime;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Schema, DesignTime);
    }
}