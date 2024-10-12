using Newtonsoft.Json.Serialization;

namespace FclEx.Json;

public class BypassConverterResolver : DefaultContractResolver
{
    private readonly ISet<Type> _converterTypes;

    public BypassConverterResolver(IEnumerable<Type> converterTypes)
    {
        _converterTypes = converterTypes.AsISet();
    }

    public BypassConverterResolver(IEnumerable<JsonConverter> converters)
        : this(converters.Select(m => m.GetType()))
    {
    }

    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        var contract = base.CreateObjectContract(objectType);
        if (contract.Converter != null && _converterTypes.Contains(contract.Converter.GetType()))
            contract.Converter = null;
        return contract;
    }
}