namespace FclEx.Utils;

public readonly record struct NameValuesBuilderOptions(
    NameValueOmitOption OmitOption,
    BoolValueConvention BoolValueConvention);