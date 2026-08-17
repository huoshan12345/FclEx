using System.Xml.Linq;
using System.Xml.Serialization;

namespace System.Xml;

public static class XmlHelper
{
    private static readonly ConditionalWeakTable<Type, XmlSerializer> XmlSerializers = new();

    private static readonly XmlWriterSettings DefaultXmlWriterSettings = new()
    {
        OmitXmlDeclaration = false,
        Indent = true,
    };

    public static T Deserialize<T>(string xml, LoadOptions options = LoadOptions.None)
    {
        var element = XElement.Parse(xml, options);
        return Deserialize<T>(element);
    }

    public static T Deserialize<T>(XElement element)
    {
        if (typeof(T) == typeof(string))
            return (T)(object)element.Value;

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (targetType.IsPrimitive
            || targetType == typeof(decimal)
            || targetType == typeof(DateTime)
            || targetType == typeof(Guid)
            )
        {
            var value = Convert.ChangeType(element.Value, targetType, CultureInfo.InvariantCulture);
            return (T)value;
        }

        var serializer = XmlSerializers.GetValue(typeof(T), t => new XmlSerializer(t));
        using var reader = element.CreateReader();
        return (T)serializer.Deserialize(reader)!;
    }

    [return: NotNullIfNotNull(nameof(obj))]
    public static string? Serialize<T>(T obj, XmlWriterSettings? settings = null)
    {
        if (obj is null)
            return null;

        var serializer = XmlSerializers.GetValue(typeof(T), t => new XmlSerializer(t));
        using var writer = new StringWriter();
        using var xmlWriter = XmlWriter.Create(writer, settings ?? DefaultXmlWriterSettings);
        serializer.Serialize(xmlWriter, obj);
        return writer.ToString();
    }
}
