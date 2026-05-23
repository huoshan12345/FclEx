namespace FclEx.NewtonsoftJson;

public readonly record struct NewtonsoftJsonOptions(
    Formatting Formatting = Formatting.None,
    bool IgnoreNull = false,
    DateTimeZoneHandling DateTimeZoneHandling = DateTimeZoneHandling.Local,
    bool CamelCase = false,
    string? DateTimeFormat = null);