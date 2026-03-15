using System.Xml.Linq;
using System.Xml.Serialization;

namespace System.Xml;

public static class XmlHelper
{
    private static readonly ConcurrentDictionary<Type, XmlSerializer> XmlSerializers = new();

    private static readonly XmlWriterSettings DefaultXmlWriterSettings = new()
    {
        OmitXmlDeclaration = false,
        Indent = true,
    };

    public static T Deserialize<T>(string xml, LoadOptions options = LoadOptions.None)
    {
        var xle = XElement.Parse(xml, options);
        return Deserialize<T>(xle);
    }

    public static T Deserialize<T>(XElement element)
    {
        var serializer = XmlSerializers.GetOrAdd(typeof(T), t => new XmlSerializer(t));
        using var reader = element.CreateReader();
        return (T)serializer.Deserialize(reader)!;
    }

    [return: NotNullIfNotNull(nameof(obj))]
    public static string? Serialize<T>(T obj, XmlWriterSettings? settings = null)
    {
        if (obj is null)
            return null;

        var serializer = XmlSerializers.GetOrAdd(typeof(T), t => new XmlSerializer(t));
        using var writer = new StringWriter();
        using var xmlWriter = XmlWriter.Create(writer, settings ?? DefaultXmlWriterSettings);
        serializer.Serialize(xmlWriter, obj);
        return writer.ToString();
    }
}
