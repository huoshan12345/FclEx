#if NET6_0_OR_GREATER
#pragma warning disable CA2252
namespace FclEx.Utils;

public interface IFromString<out T> where T : IFromString<T>
{
    static abstract T? FromString(string? str);
}
#endif
