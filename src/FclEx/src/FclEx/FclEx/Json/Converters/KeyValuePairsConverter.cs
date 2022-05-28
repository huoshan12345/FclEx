using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using FclEx.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FclEx.Json.Converters;

public class KeyValuePairsConverter : JsonConverter
{
    public override bool CanRead { get; } = true;
    public override bool CanWrite { get; } = false;

    private static readonly MethodInfo MethodOfToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))!;

    public override bool CanConvert(Type objectType)
    {
        var colItemType = objectType.EnumerableType();
        if (colItemType == null) return false;
        var kvType = colItemType.GetGenericTypeDefinition();
        if (kvType != typeof(KeyValuePair<,>)) return false;
        var keyType = colItemType.GenericTypeArguments[0];
        return keyType.IsPrimitive || keyType.IsEnum || keyType == typeof(string);
    }

    private static object? Convert(Type objectType, Type eleType, IList list)
    {
        if (objectType.IsArray)
        {
            return MethodOfToArray.MakeGenericMethod(eleType).Invoke(null, new object[] { list });
        }

        var colType = objectType.GetGenericTypeDefinition();
        if (objectType.IsInstanceOfType(list)) return list;

        if (!colType.IsAbstract)
        {
            if (colType == typeof(ReadOnlyCollection<>)) return objectType.CreateObject(list);

            var ctor = objectType.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(eleType) });
            if (ctor != null) return ctor.Invoke(new object[] { list });

            if (colType.IsInheritedFromGenericType(typeof(ICollection<>)))
            {
                var obj = objectType.CreateObject();
                var addMethod = objectType.GetMethod(nameof(ICollection<object>.Add)) ?? throw new MissingMethodException("Cannot find a method named Add");
                foreach (var item in list)
                {
                    addMethod.Invoke(obj, new[] { item });
                }
                return obj;
            }
        }
        // just cast
        return list;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    private static object GetActualValue(Type t, string value)
    {
        if (t == typeof(string)) return value;
        else if (t == typeof(char)) return value[0];
        else return JsonConvert.DeserializeObject(value, t)!;
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;

        var kvType = objectType.EnumerableType()!;
        var keyType = kvType.GenericTypeArguments[0];
        var valueType = kvType.GenericTypeArguments[1];

        var list = (IList)typeof(List<>).MakeGenericType(kvType).CreateObject()!;
        var pairCtor = kvType.GetConstructor(kvType.GenericTypeArguments) ?? throw new MissingMethodException("Can not find a suitable constructor");

        var token = JToken.ReadFrom(reader);

        foreach (var (s, jToken) in token.ToJObject()!)
        {
            var key = GetActualValue(keyType, s);
            var value = jToken?.ToObject(valueType);
            var pair = pairCtor.Invoke(new[] { key, value });
            list.Add(pair);
        }
        return Convert(objectType, kvType, list);
    }
}