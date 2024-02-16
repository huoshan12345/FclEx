namespace FclEx.Extensions;

public static class NullableExtensions
{
    /// <summary>
    /// Exactly same as GetValueOrDefault but with shorter name.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static T Get<T>(this T? t, T defaultValue = default) where T : struct
    {
        return t.GetValueOrDefault(defaultValue);
    }

    /// <summary>
    /// Indicates whether the specified value is both not <see langword="null" /> and not <see langword="default" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsValid<T>(this T? value) where T : struct
    {
        return !value.IsNullOrDefault();
    }

    public static bool IsNullOrDefault<T>(this T? t) where T : struct
    {
        return EqualityComparer<T>.Default.Equals(t.Get(), default);
    }
}