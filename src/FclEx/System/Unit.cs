using FclEx.Json.Converters;
using Newtonsoft.Json;

namespace System
{
    [JsonConverter(typeof(IgnoreJsonConverter))]
    public readonly struct Unit { }
}