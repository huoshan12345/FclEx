namespace FclEx.Utils;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class NameValueAttribute(string? name) : Attribute
{
    // ReSharper disable once UnusedMember.Global
    public NameValueAttribute() : this(null) { }

    public string? Name { get; set; } = name;

    public NameValueOmitOption OmitOption { get; set; }

    public BoolValueConvention BoolValueConvention { get; set; }
}