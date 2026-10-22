namespace FclEx.Domain;

/// <summary>Excludes a field or property from entity data equality and hash-code calculation.</summary>
/// <remarks>
/// Applies to comparers created by <see cref="EntityDataEqualityComparer"/>, including comparers with a custom
/// exclusion list. An inherited member retains its exclusion, but an overriding property must be marked again.
/// This attribute does not affect the entity's own Equals or GetHashCode implementation or other comparers.
/// </remarks>
// ReSharper disable once RedundantAttributeUsageProperty
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class EntityDataEqualityIgnoreAttribute : Attribute;
