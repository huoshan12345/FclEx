namespace FclEx.Json;

public class IgnoreTypesJsonConverter : IgnoreJsonConverter
{
    private readonly IReadOnlyCollection<Type> _ignoreTypes;

    public IgnoreTypesJsonConverter(IEnumerable<Type> ignoreTypes)
    {
        _ignoreTypes = ignoreTypes.AsIReadOnlyCollection();
    }

    public IgnoreTypesJsonConverter(params Type[] ignoreTypes) : this(ignoreTypes.AsEnumerable())
    {
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return _ignoreTypes.Any(m => m.IsAssignableFrom(typeToConvert));
    }
}