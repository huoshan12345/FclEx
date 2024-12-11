namespace FclEx.Utils;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class UriParamAttribute(string? name) : Attribute
{
    // ReSharper disable once UnusedMember.Global
    public UriParamAttribute() : this(null) { }

    public string? Name { get; set; } = name;

    public UriParamOmitOption OmitOption { get; set; }

    public BoolValueConvention BoolValueConvention { get; set; }
}