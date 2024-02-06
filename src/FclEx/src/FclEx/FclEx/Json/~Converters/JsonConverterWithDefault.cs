namespace FclEx.Json;

public abstract class JsonConverterWithDefault<TSelf> : JsonConverter where TSelf : JsonConverter
{
    public static DefaultResolver<TSelf> DefaultResolver { get; } = new();
    public static JsonSerializerSettings DefaultSettings { get; } = new() { ContractResolver = DefaultResolver };
    public static JsonSerializer DefaultSerializer { get; } = JsonSerializer.Create(DefaultSettings);
}