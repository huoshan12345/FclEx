namespace FclEx.NewtonsoftJson;

public static class NewtonsoftJsonHelper
{
    public static IContractResolver CamelResolver { get; } = new DefaultContractResolver
    {
        NamingStrategy = new CamelCaseNamingStrategy()
    };

    private static readonly ConcurrentDictionary<NewtonsoftJsonOptions, JsonSerializerSettings> _serializerSettings = new();
    private static readonly ConcurrentDictionary<NewtonsoftJsonOptions, JsonSerializer> _serializers = new();

    public static JsonSerializerSettings GetOptions(NewtonsoftJsonOptions options)
    {
        return _serializerSettings.GetOrAdd(options, k =>
        {
            var settings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = k.DateTimeZoneHandling,
                Formatting = k.Formatting,
                NullValueHandling = k.IgnoreNull ? NullValueHandling.Ignore : NullValueHandling.Include,
            };
            if (k.DateTimeFormat.IsNotEmpty())
                settings.DateFormatString = k.DateTimeFormat!;
            if (k.CamelCase)
                settings.ContractResolver = CamelResolver;
            return settings;
        });
    }

    public static JsonSerializer CamelSerializer { get; } = GetSerializer(new NewtonsoftJsonOptions(CamelCase: true));

    public static JsonSerializer GetSerializer(NewtonsoftJsonOptions options)
    {
        return _serializers.GetOrAdd(options, k => JsonSerializer.Create(GetOptions(k)));
    }
}