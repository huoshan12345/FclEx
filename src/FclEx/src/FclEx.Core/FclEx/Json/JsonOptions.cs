namespace FclEx.Json;

public readonly record struct JsonOptions(
    bool Indented = false,
    bool IgnoreNull = false,
    bool RelaxedEscaping = false,
    bool PropertyNameCaseInsensitive = false,
    JsonNamingPolicy? PropertyNamingPolicy = null,
    JsonNumberHandling NumberHandling = JsonNumberHandling.Strict);