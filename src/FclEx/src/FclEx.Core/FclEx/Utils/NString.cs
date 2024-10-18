namespace FclEx.Utils;

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
    public static bool IsNotEmpty(this NString nstr) => nstr.Value.IsNotEmpty();
}

public class NStringJsonConverter : JsonConverter<NString>
{
    public override NString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return reader.TokenType switch
        {
            JsonTokenType.String => new NString(reader.GetString()),
            JsonTokenType.Null => new NString(null),
            _ => throw new InvalidOperationException("Cannot read json with this converter for token type: " + reader.TokenType),
        };
    }

    public override void Write(Utf8JsonWriter writer, NString value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}