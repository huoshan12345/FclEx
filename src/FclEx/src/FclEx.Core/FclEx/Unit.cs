using System.Text.Json.Serialization.Converters;

namespace FclEx
{
    [JsonConverter(typeof(IgnoreJsonConverter))]
    public readonly record struct Unit
    {
        public static readonly Unit Default = default;

        public override int GetHashCode() => 0;
        public override string ToString() => "()";
    }
}