namespace Xunit;

public static class Extensions
{
    public static TheoryData<T> ToTheoryData<T>(this IEnumerable<T> enumerable)
    {
        return [.. enumerable];
    }

    /// <summary>
    /// Converts a value to a string suitable for use in assertion messages.
    /// </summary>
    /// <remarks>
    /// If the value is already a <see cref="string"/>, it is returned unchanged.
    /// Otherwise, the value is formatted using xUnit's argument formatter.
    /// </remarks>
    public static string ToAssertionString<T>(this T? value)
    {
        return value as string ?? ArgumentFormatter.Format(value);
    }

#if FCLEX_XUNIT_V3
    public static void Add<T>(this TheoryData<T> data, T item)
    {
        data.Add(new TheoryDataRow<T>(item));
    }
#endif

}
