namespace FclEx.Utils;

[DebuggerDisplay("{" + nameof(Value) + "}")]
[JsonConverter(typeof(NStringJsonConverter))]
public readonly struct NString(string? value) : IEquatable<NString>
{
    public string Value => value ?? string.Empty; // use compute property to avoid null when create default struct
    public int Length => Value.Length;

    public char this[int index] => Value[index];
    public override string ToString() => Value;
    public bool Equals(NString other) => Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
    public CharEnumerator GetEnumerator() => Value.GetEnumerator();

    public static implicit operator string(NString str) => str.Value;
    public static implicit operator NString(string? str) => new(str);

    public static bool operator ==(NString left, string right) => left == (NString)right;
    public static bool operator !=(NString left, string right) => !(left == right);

    public static bool operator ==(NString left, NString right) => left.Equals(right);
    public static bool operator !=(NString left, NString right) => !(left == right);

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            NString other => Value == other.Value,
            string other => Value == other,
            _ => false,
        };
    }
}

public static class NStringExtensions
{
    public static bool IsNotEmpty(this NString str) => str.Value.IsNotEmpty();
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