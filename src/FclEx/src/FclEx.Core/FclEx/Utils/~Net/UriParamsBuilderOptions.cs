namespace FclEx.Utils;

public readonly record struct UriParamsBuilderOptions(
    UriParamOmitOption OmitOption,
    BoolValueConvention BoolValueConvention);