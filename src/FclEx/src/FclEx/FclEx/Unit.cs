using FclEx.Json.Converters;
using Newtonsoft.Json;

namespace FclEx;

[JsonConverter(typeof(IgnoreJsonConverter))]
public readonly struct Unit { }