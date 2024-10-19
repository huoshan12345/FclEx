namespace FclEx.Json;

public readonly record struct JsonOptions(
    bool Indented = false,
    bool IgnoreNull = false,
    JsonNamingPolicy? PropertyNamingPolicy = null,
    bool PropertyNameCaseInsensitive = false);