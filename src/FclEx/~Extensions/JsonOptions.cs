using System;
using Newtonsoft.Json;

namespace FclEx
{
    public readonly struct JsonOptions : IEquatable<JsonOptions>
    {
        public JsonOptions(Formatting formatting = Formatting.None,
            bool ignoreNull = false,
            DateTimeZoneHandling dateTimeZoneHandling = DateTimeZoneHandling.Local,
            bool useCamelCase = false,
            string? dateTimeFormat = null)
        {
            Formatting = formatting;
            IgnoreNull = ignoreNull;
            DateTimeZoneHandling = dateTimeZoneHandling;
            UseCamelCase = useCamelCase;
            DateTimeFormat = dateTimeFormat;
        }

        public Formatting Formatting { get; }
        public bool IgnoreNull { get; }
        public DateTimeZoneHandling DateTimeZoneHandling { get; }
        public bool UseCamelCase { get; }
        public string? DateTimeFormat { get; }

        public bool Equals(JsonOptions other)
        {
            return Formatting == other.Formatting
                   && IgnoreNull == other.IgnoreNull
                   && DateTimeZoneHandling == other.DateTimeZoneHandling
                   && UseCamelCase == other.UseCamelCase
                   && string.Equals(DateTimeFormat, other.DateTimeFormat);
        }

        public override bool Equals(object obj)
        {
            return obj is JsonOptions other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Formatting;
                hashCode = (hashCode * 397) ^ IgnoreNull.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)DateTimeZoneHandling;
                hashCode = (hashCode * 397) ^ UseCamelCase.GetHashCode();
                hashCode = (hashCode * 397) ^ DateTimeFormat.GetHashCodeSafely();
                return hashCode;
            }
        }

        public static bool operator ==(JsonOptions left, JsonOptions right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(JsonOptions left, JsonOptions right)
        {
            return !left.Equals(right);
        }
    }
}