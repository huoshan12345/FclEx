using System;
using Newtonsoft.Json.Linq;

namespace FclEx.Json.Converters;

public class ReadSingleOrLastConverter : ReadSingleConverter
{
    protected override Func<JArray, JToken?> SingleFunc { get; } = array => array.Last;
}