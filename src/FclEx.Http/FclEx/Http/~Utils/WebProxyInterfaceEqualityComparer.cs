namespace FclEx.Http;

/// <summary>
/// Compares <see cref="IWebProxy"/> values, using value equality for <see cref="WebProxy"/> instances.
/// </summary>
/// <remarks>Custom <see cref="IWebProxy"/> implementations are only equal when they are the same instance.</remarks>
public class WebProxyInterfaceEqualityComparer : IEqualityComparer<IWebProxy>
{
    /// <summary>
    /// A shared comparer instance.
    /// </summary>
    public static readonly WebProxyInterfaceEqualityComparer Instance = new();

    private static IEqualityComparer<WebProxy> WebProxyComparer
        => WebProxyEqualityComparer.Instance;

    public bool Equals(IWebProxy? x, IWebProxy? y)
    {
        if (Comparer.TryEquals(x, y, out var result))
            return result.Value;

        if (x is WebProxy wx && y is WebProxy wy)
            return WebProxyComparer.Equals(wx, wy);

        return false;
    }

    public int GetHashCode(IWebProxy obj)
    {
        if (obj is WebProxy webProxy)
            return WebProxyComparer.GetHashCode(webProxy);

        return obj.GetHashCode();
    }
}
