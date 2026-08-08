namespace FclEx.EfCore;

/// <summary>
/// Controls whether unique indexes declared on an entity are extended with its soft-delete properties.
/// </summary>
/// <remarks>
/// Entities are processed by default. Apply this attribute with <see langword="false"/> to leave the entity's indexes unchanged.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ConfigureSoftDeleteIndexesAttribute(bool enabled = true) : Attribute
{
    /// <summary>
    /// Gets whether soft-delete properties should be added to the entity's unique indexes.
    /// </summary>
    public bool Enabled { get; } = enabled;
}
