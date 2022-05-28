using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace FclEx.Json.Converters;

public class ReadSingleOrFirstConverter : ReadSingleConverter
{
    protected override Func<JArray, JToken?> SingleFunc { get; } = array => array.First;
}