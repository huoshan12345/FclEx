using FclEx.Json.Converters;
using Newtonsoft.Json;

namespace FclEx.Utils
{
    [JsonConverter(typeof(IgnoreJsonConverter))]
    public struct Unit { }
}