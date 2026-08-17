namespace FclEx.Utils;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class NameValueAttribute(string? name) : Attribute
{
    // ReSharper disable once UnusedMember.Global
    public NameValueAttribute() : this(null) { }

    public string? Name { get; set; } = name;

    public NameValueOmitOption OmitOption { get; set; }

    public BoolValueConvention BoolValueConvention { get; set; }

    /// <summary>
    /// Gets or sets the standard or custom format string passed to <see cref="INameValuesBuilder.ToString{T}(T, string)"/>
    /// when this member is added to the name-value collection.
    /// </summary>
    public string? Format { get; set; }
}
