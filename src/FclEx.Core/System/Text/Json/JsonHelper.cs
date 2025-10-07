namespace System.Text.Json;

public static class JsonHelper
{
    private static readonly ConcurrentDictionary<JsonOptions, JsonSerializerOptions> _serializerOptions = new();

    private static readonly DefaultJsonTypeInfoResolver Resolver = new()
    {
        Modifiers =
        {
            IncludeStaticMembers,
            IgnoreEmptyValue,
        },
    };

    private static readonly DefaultJsonTypeInfoResolver IgnoreReadingNullResolver = new()
    {
        Modifiers =
        {
            IncludeStaticMembers,
            IgnoreEmptyValue,
            IgnoreReadingNull,
        },
    };

    public static JsonSerializerOptions GetOptions(JsonOptions? options = default)
    {
        options ??= JsonOptions.Default;
        return _serializerOptions.GetOrAdd(options, Create);

        static JsonSerializerOptions Create(JsonOptions k)
        {
            var options = new JsonSerializerOptions
            {
                AllowOutOfOrderMetadataProperties = k.AllowOutOfOrderMetadataProperties,
                PropertyNameCaseInsensitive = k.PropertyNameCaseSensitive == false,
                DefaultIgnoreCondition = k.IgnoreWritingNull
                    ? JsonIgnoreCondition.WhenWritingNull
                    : JsonIgnoreCondition.Never,
                WriteIndented = k.Indented,
                PropertyNamingPolicy = k.PropertyNamingPolicy,
                Encoder = k.StrictEscaping ? null : RelaxedEncoder.Instance,
                NumberHandling = k.AllowNumberFromString
                    ? JsonNumberHandling.AllowReadingFromString
                    : JsonNumberHandling.Strict,
                TypeInfoResolver = k.IgnoreReadingNull
                    ? IgnoreReadingNullResolver
                    : Resolver,
            };

            if (k.AllowBoolFromString)
                options.Converters.Add(BooleanJsonConverter.Instance);

            options.MakeReadOnly(true);
            return options;
        }
    }

    /// <summary>
    /// Configures a <see cref="JsonTypeInfo"/> to ignore properties with empty values during JSON serialization.
    /// </summary>
    /// <param name="typeInfo">
    /// The <see cref="JsonTypeInfo"/> object representing metadata about the type being serialized.
    /// </param>
    /// <remarks>
    /// This method iterates through the properties of the given <paramref name="typeInfo"/> and modifies their
    /// <c>ShouldSerialize</c> delegate to exclude properties with empty enumerable values from serialization.<br/>
    /// <br/>
    /// A property is ignored if:<br/>
    /// 1. Its type implements <see cref="IEnumerable"/>.<br/>
    /// 2. It is annotated with the <see cref="JsonIgnoreEmptyAttribute"/>.<br/>
    /// <br/>
    /// Example:
    /// If a property is an empty list or collection, it will not be included in the JSON output if the
    /// <see cref="JsonIgnoreEmptyAttribute"/> is applied to it.
    /// </remarks>
    public static void IgnoreEmptyValue(JsonTypeInfo typeInfo)
    {
        var ignore = typeInfo.Type.IsDefined<JsonIgnoreEmptyAttribute>(true);
        foreach (var property in typeInfo.Properties)
        {
            var type = property.PropertyType;
            // string or collection
            if (type.IsEnumerable() && (ignore || property.IsDefined<JsonIgnoreEmptyAttribute>(true)))
            {
                property.ShouldSerialize = (_, val) => ((IEnumerable?)val).IsNullOrEmpty() == false;
            }
        }
    }

    /// <summary>
    /// Adds static members of a type as properties to the <see cref="JsonTypeInfo"/> for serialization.
    /// </summary>
    /// <param name="typeInfo">
    /// The <see cref="JsonTypeInfo"/> object representing metadata about the type being serialized.
    /// </param>
    /// <remarks>
    /// This method is a workaround for <see cref="System.Text.Json"/>'s limitation where static members are not 
    /// included in serialization by default. It adds static members as properties to the serialization process if:
    /// 1. The member is static.
    /// 2. The member is annotated with the <see cref="JsonIncludeAttribute"/>.
    /// 
    /// The method retrieves the name of each static member, either from the <see cref="JsonPropertyNameAttribute"/> 
    /// or by applying the <see cref="JsonSerializerOptions.PropertyNamingPolicy"/>. It then creates a <see cref="JsonPropertyInfo"/> 
    /// for each member, setting up a getter to return the value of the static member and an optional custom converter 
    /// if specified by the <see cref="JsonConverterAttribute"/>. 
    /// 
    /// The static member is added to the <paramref name="typeInfo"/>'s property list, making it available for serialization.
    /// </remarks>
    public static void IncludeStaticMembers(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        var members = typeInfo.Type.GetDataMembers();
        foreach (var member in members.Where(m => m.IsStatic).Where(m => m.IsDefined<JsonIncludeAttribute>(false)))
        {
            var name = member.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                       ?? typeInfo.Options.PropertyNamingPolicy?.ConvertName(member.Name)
                       ?? member.Name;

            var value = member.GetValue(null);
            var propertyInfo = typeInfo.CreateJsonPropertyInfo(value?.GetType() ?? member.DataMemberType, name);
            propertyInfo.Get = (o) => value;
            propertyInfo.CustomConverter = member.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType is { } converterType
                ? (JsonConverter?)Activator.CreateInstance(converterType)
                : null;

            typeInfo.Properties.Add(propertyInfo);
        }
    }

    public static void IgnoreReadingNull(JsonTypeInfo typeInfo)
    {
        foreach (var propertyInfo in typeInfo.Properties)
        {
            var setter = propertyInfo.Set;
            if (setter is null)
                continue;

            propertyInfo.Set = (obj, value) =>
            {
                if (value != null)
                {
                    setter(obj, value);
                }
            };
        }
    }
}