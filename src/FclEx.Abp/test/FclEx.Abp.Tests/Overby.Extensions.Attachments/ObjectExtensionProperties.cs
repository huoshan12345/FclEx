namespace Overby.Extensions.Attachments;

/// <summary>
/// Examples of extension properties on System.Object.
/// </summary>
/// <remarks>
/// Tests are down below.
/// </remarks>
internal static class ObjectExtensionProperties
{
    public static ExtensionProperty<int?> Id(this object obj, Optional<int?> value = default) =>
        obj.GetExtensionProperty(value);

    public static ExtensionProperty<string> Name(this object obj, Optional<string> value = default) =>
        obj.GetExtensionProperty(value);

    public static ExtensionProperty<string> Description(this object obj, Optional<string> value = default) =>
        obj.GetExtensionProperty(value);

    public static ExtensionProperty<DateTimeOffset> Expiry(this object obj, Optional<DateTimeOffset> value = default) =>
        obj.GetExtensionProperty(value);

    /// <summary>
    /// Example of a read only "property" based on other extension properties.
    /// This is actually nothing special at all!
    /// </summary>
    public static bool IsExpired(this object obj) =>
        DateTimeOffset.Now < obj.Expiry();
}