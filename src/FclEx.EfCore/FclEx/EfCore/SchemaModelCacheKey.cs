using System;

namespace FclEx.EfCore;

/// <summary>
/// Identifies a cached EF Core model by context type, schema, and design-time mode.
/// </summary>
public class SchemaModelCacheKey : ModelCacheKey, IHasSchema
{
    /// <inheritdoc />
    public string? Schema { get; }

    /// <summary>
    /// Gets whether the model is being created for design-time tooling.
    /// </summary>
    public bool DesignTime { get; }

    /// <summary>
    /// Initializes a model cache key for a context, schema, and design-time mode.
    /// </summary>
    /// <param name="context">The context whose type contributes to the key.</param>
    /// <param name="schema">The schema that contributes to the key.</param>
    /// <param name="designTime">Whether the model is being created for design-time tooling.</param>
    public SchemaModelCacheKey(DbContext context, string? schema, bool designTime) : base(context)
    {
        DesignTime = designTime;
        Schema = schema;
    }

    /// <inheritdoc />
    protected override bool Equals(ModelCacheKey key)
    {
        return key is SchemaModelCacheKey other
               && base.Equals(other)
               && Schema == other.Schema
               && DesignTime == other.DesignTime;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Schema, DesignTime);
    }
}
