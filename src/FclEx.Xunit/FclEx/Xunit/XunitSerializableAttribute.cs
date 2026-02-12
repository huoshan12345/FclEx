namespace FclEx.Xunit;

/// <summary>
/// Indicates that a type should have an <see cref="IXunitSerializable"/>
/// implementation generated at compile time.
/// </summary>
/// <remarks>
/// The source generator will:
/// <list type="bullet">
/// <item>
/// <description>
/// Generate an implementation of <see cref="IXunitSerializable"/>
/// for the annotated type.
/// </description>
/// </item>
/// <item>
/// <description>
/// Generate a parameterless constructor if one is not already declared.
/// </description>
/// </item>
/// <item>
/// <description>
/// Serialize and deserialize all instance fields using reflection.
/// </description>
/// </item>
/// </list> 
/// The annotated type must be declared as <see langword="partial"/>.<br/>
/// If the annotated type is nested, all containing types must also be declared as <see langword="partial"/>.<br/>
/// <br/>
/// Struct types are not supported because <see cref="IXunitSerializable"/>
/// requires mutating an existing instance during deserialization.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)] // Not supported for structs since IXunitSerializable.Deserialize relies on mutation semantics.
public sealed class XunitSerializableAttribute : Attribute;
