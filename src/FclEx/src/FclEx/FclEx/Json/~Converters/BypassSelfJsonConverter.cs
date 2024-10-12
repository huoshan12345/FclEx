using Newtonsoft.Json.Serialization;

namespace FclEx.Json;

public abstract class BypassSelfJsonConverter : JsonConverter
{
    protected static JsonSerializer CreateSerializer(IContractResolver resolver)
    {
        return JsonSerializer.Create(new JsonSerializerSettings { ContractResolver = resolver });
    }

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