namespace FclEx.Json;

/// <summary>
/// Specifies that the target property or field should be ignored during serialization if it contains an empty value.<br/>
/// For example, an empty string or an empty collection will not be serialized.
/// </summary>
/// <remarks>
/// This attribute is enabled by default for <see cref="JsonSerializerOptions"/> instances created by <see cref="JsonHelper"/>.<br/>
/// To enable this behavior for custom <see cref="JsonSerializerOptions"/>, use the extension method <see cref="JsonSerializerOptionsExtensions.AddModifierForEmptyValue"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class JsonIgnoreEmptyAttribute : Attribute;