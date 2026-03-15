namespace System.Text.Json;

public record JsonOptions(
    bool Indented = false,
    bool IgnoreWritingNull = false,
    bool IgnoreReadingNull = true,
    bool StrictEscaping = false,
    bool PropertyNameCaseSensitive = false,
    bool AllowBoolFromString = true,
    bool AllowNumberFromString = true,
    bool AllowOutOfOrderMetadataProperties = true,
    JsonNamingPolicy? PropertyNamingPolicy = null,
    bool AddTypeConverter = true,
    bool AddObjectConverter = true)
{
    public static readonly JsonOptions Default = new();
    public static readonly JsonOptions Web = new(PropertyNamingPolicy: JsonNamingPolicy.CamelCase);
}