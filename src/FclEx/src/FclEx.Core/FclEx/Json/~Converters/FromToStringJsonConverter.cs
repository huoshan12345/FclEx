#if NET6_0_OR_GREATER
namespace FclEx.Json;

public class FromToStringJsonConverter<T> : StringJsonConverter<T> where T : IFromString<T>
{
    public FromToStringJsonConverter() : base(T.FromString, m => m?.ToString())
    {
    }
}
#endif