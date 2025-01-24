#if NET6_0_OR_GREATER
namespace System.Text.Json.Serialization;

public class FromToStringJsonConverter<T> : StringJsonConverter<T> where T : IFromString<T>
{
    public FromToStringJsonConverter() : base(T.FromString, m => m?.ToString())
    {
    }
}
#endif