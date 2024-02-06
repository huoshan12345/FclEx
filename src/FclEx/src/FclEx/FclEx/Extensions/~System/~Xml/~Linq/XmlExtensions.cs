using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using MoreLinq.Extensions;
using Formatting = Newtonsoft.Json.Formatting;

namespace FclEx.Extensions;

public static class XmlExtensions
{
    public static string ToJson(this XNode xml, Formatting formatting = Formatting.None, bool omitRootObject = false)
    {
        return JsonConvert.SerializeXNode(xml, formatting, omitRootObject);
    }

    public static string ToJson(this XmlNode xml, Formatting formatting = Formatting.None, bool omitRootObject = false)
    {
        return JsonConvert.SerializeXmlNode(xml, formatting, omitRootObject);
    }
}