using System.Collections.ObjectModel;

namespace FclEx.NewtonsoftJson;

public class KeyValuePairsConverter : BypassSelfJsonConverter
{
    public override bool CanRead { get; } = true;
    public override bool CanWrite { get; } = false;

    private static readonly MethodInfo MethodOfToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))!;

    public override bool CanConvert(Type objectType)
    {
        var elementType = objectType.EnumerableType();
        if (elementType == null)
            return false;

        var kvType = elementType.GetGenericTypeDefinition();
        if (kvType != typeof(KeyValuePair<,>))
            return false;

        var keyType = elementType.GenericTypeArguments[0];
        return keyType.IsPrimitive || keyType.IsEnum || keyType == typeof(string);
    }

    private static object? Convert(Type objectType, Type eleType, IEnumerable list)
    {
        if (objectType.IsArray)
        {
            return MethodOfToArray.MakeGenericMethod(eleType).Invoke(null, [list]);
        }

        var colType = objectType.GetGenericTypeDefinition();

        if (objectType.IsInstanceOfType(list) || colType.IsAbstract)
            return list;

        if (colType == typeof(ReadOnlyCollection<>))
            return objectType.CreateObject(list);

        var ctor = objectType.GetConstructor([typeof(IEnumerable<>).MakeGenericType(eleType)]);
        if (ctor != null) return ctor.Invoke([list]);

        if (colType.Implements(typeof(ICollection<>)) == false)
            return list;

        var obj = objectType.CreateObject();
        var addMethod = objectType.GetMethod(nameof(ICollection<object>.Add)) ?? throw new MissingMethodException("Cannot find a method named Add");
        foreach (var item in list)
        {
            addMethod.Invoke(obj, [item]);
        }

        return obj;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    private static object GetActualValue(Type t, string value)
    {
        if (t == typeof(string))
            return value;

        if (t == typeof(char))
            return value[0];

        return JsonConvert.DeserializeObject(value, t)!;
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var token = JToken.ReadFrom(reader);

        if (token.Type == JTokenType.Array)
        {
            return token.ToObject(objectType, BypassSerializer);
        }

        var kvType = objectType.EnumerableType()!;
        var keyType = kvType.GenericTypeArguments[0];
        var valueType = kvType.GenericTypeArguments[1];

        var list = (IList)typeof(List<>).MakeGenericType(kvType).CreateObject()!;
        var pairCtor = kvType.GetConstructor(kvType.GenericTypeArguments)
                       ?? throw new MissingMethodException("Can not find a suitable constructor");



        foreach (var (s, jToken) in token.ToJObject()!)
        {
            var key = GetActualValue(keyType, s);
            var value = jToken?.ToObject(valueType);
            var pair = pairCtor.Invoke([key, value]);
            list.Add(pair);
        }

        return Convert(objectType, kvType, list);
    }
}