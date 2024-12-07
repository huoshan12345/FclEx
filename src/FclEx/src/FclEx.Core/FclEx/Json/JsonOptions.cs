namespace FclEx.Json;

public readonly record struct JsonOptions(
    bool Indented = false,
    bool IgnoreNull = false,
    bool StrictEscaping = false,
    bool PropertyNameCaseSensitive = false,
    bool DisallowBoolFromString = false,
    bool DisallowNumberFromString = false,
    JsonNamingPolicy? PropertyNamingPolicy = null);