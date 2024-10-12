namespace FclEx.Json;

public abstract class BypassSelfJsonConverter : JsonConverter
{
    protected readonly Lazy<IContractResolver> _resolver;
    protected readonly Lazy<JsonSerializerSettings> _settings;
    protected readonly Lazy<JsonSerializer> _serializer;

    public BypassSelfJsonConverter()
    {
        _resolver = new(() => new BypassConverterResolver([this]));
        _settings = new(() => new() { ContractResolver = _resolver.Value });
        _serializer = new(() => JsonSerializer.Create(_settings.Value));
    }

    public IContractResolver BypassContractResolver => _resolver.Value;
    public JsonSerializerSettings BypassSettings => _settings.Value;
    public JsonSerializer BypassSerializer => _serializer.Value;
}

public abstract class BypassSelfJsonConverter<T> : JsonConverter<T>
{
    protected readonly Lazy<IContractResolver> _resolver;
    protected readonly Lazy<JsonSerializerSettings> _settings;
    protected readonly Lazy<JsonSerializer> _serializer;

    public BypassSelfJsonConverter()
    {
        _resolver = new(() => new BypassConverterResolver([this]));
        _settings = new(() => new() { ContractResolver = _resolver.Value });
        _serializer = new(() => JsonSerializer.Create(_settings.Value));
    }

    public IContractResolver BypassContractResolver => _resolver.Value;
    public JsonSerializerSettings BypassSettings => _settings.Value;
    public JsonSerializer BypassSerializer => _serializer.Value;
}