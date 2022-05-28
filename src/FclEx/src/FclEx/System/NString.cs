using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using FclEx;
using FclEx.Extensions;
using Newtonsoft.Json;

namespace System;

[DebuggerDisplay("{" + nameof(Value) + "}")]
[JsonConverter(typeof(NStringJsonConverter))]
public readonly struct NString : IEquatable<NString>
{
    private readonly string? _value;
    public string Value => _value ?? string.Empty;

    public NString(string? inner)
    {
        _value = inner;
    }

    public static implicit operator string(NString nstr) => nstr.Value;
    public static implicit operator NString(string? str) => new(str);

    public override string ToString() => Value;
    public bool Equals(NString other) => Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(NString left, NString right) => left.Equals(right);
    public static bool operator !=(NString left, NString right) => !(left == right);

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            NString other => Value == other.Value,
            string other => Value == other,
            _ => false
        };
    }
}

public static class NStringExtensions
{
    public static bool IsValid(this NString nstr) => ((string)nstr).IsValid();
}

public class NStringJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        switch (value)
        {
            case null:
                writer.WriteValue("");
                break;
            case NString nstr:
                writer.WriteValue(nstr.Value);
                break;
            default:
                throw new InvalidOperationException("Cannot write json with this converter for value: " + value);
        }
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        return reader.TokenType switch
        {
            JsonToken.String => new NString((string)reader.Value!),
            JsonToken.Null => new NString(null),
            _ => throw new InvalidOperationException("Cannot read json with this converter for token type: " + reader.TokenType),
        };
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(NString);
    }
}