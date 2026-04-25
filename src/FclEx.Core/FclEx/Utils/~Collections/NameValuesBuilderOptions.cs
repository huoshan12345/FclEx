namespace FclEx.Utils;

public readonly record struct NameValuesBuilderOptions(
    NameValueOmitOption OmitOption,
    BoolValueConvention BoolValueConvention)
{
    public static readonly NameValuesBuilderOptions Default = new(NameValueOmitOption.Never, BoolValueConvention.AsString);
}