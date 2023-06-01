// For background refer to this article by Dave Transom
// http://www.singular.co.nz/2008/07/finding-preferred-accept-encoding-header-in-csharp/

using System.Diagnostics;
using System.Globalization;

namespace FclEx.Http;

/// <summary>
/// Represents a weighted value (or quality value) from an http header e.g. gzip=0.9; deflate; x-gzip=0.5;
/// </summary>
/// <remarks>
/// accept-encoding spec: 
///		http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html
/// </remarks>
/// <example>
/// Accept:          text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5
/// Accept-Encoding: gzip,deflate
/// Accept-Charset:  ISO-8859-1,utf-8;q=0.7,*;q=0.7
/// Accept-Language: en-us,en;q=0.5
/// </example>
[DebuggerDisplay("QValue[{Name}, {Weight}]")]
public struct HttpQualityValue : IComparable<HttpQualityValue>
{
    private static readonly char[] _delimiters = { ';', '=' };
    private const float DefaultWeight = 1;

    #region Fields

    private string _name;
    private float _weight;

    #endregion

    #region Constructors

    public HttpQualityValue(string value)
    {
        _name = string.Empty;
        _weight = 0;
        ParseInternal(ref this, value);
    }

    #endregion

    #region Properties

    /// <summary>
    /// The name of the value part
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// The weighting (or qvalue, quality value) of the encoding
    /// </summary>
    public float Weight => _weight;

    /// <summary>
    /// Whether the value can be accepted 
    /// i.e. it's weight is greater than zero
    /// </summary>
    public bool CanAccept => _weight > 0;

    /// <summary>
    /// Whether the value is empty (i.e. has no name)
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(_name);

    #endregion

    #region Methods

    /// <summary>
    /// Parses the given string for name and 
    /// weigth (qvalue)
    /// </summary>
    /// <param name="value">The string to parse</param>
    public static HttpQualityValue Parse(string value)
    {
        var item = new HttpQualityValue();
        ParseInternal(ref item, value);
        return item;
    }

    /// <summary>
    /// Parses the given string for name and 
    /// weigth (qvalue)
    /// </summary>
    /// <param name="target"></param>
    /// <param name="value">The string to parse</param>
    private static void ParseInternal(ref HttpQualityValue target, string value)
    {
        string[] parts = value.Split(_delimiters, 3);
        if (parts.Length > 0)
        {
            target._name = parts[0].Trim();
            target._weight = DefaultWeight;
        }

        if (parts.Length == 3)
        {
            float.TryParse(parts[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out target._weight);
        }
    }

    #endregion

    #region IComparable<QValue> Members

    /// <summary>
    /// Compares this instance to another QValue by
    /// comparing first weights, then ordinals.
    /// </summary>
    /// <param name="other">The QValue to compare</param>
    /// <returns></returns>
    public int CompareTo(HttpQualityValue other)
    {
        return _weight.CompareTo(other._weight);
    }

    #endregion

    #region CompareByWeight

    /// <summary>
    /// Compares two QValues in ascending order.
    /// </summary>
    /// <param name="x">The first QValue</param>
    /// <param name="y">The second QValue</param>
    /// <returns></returns>
    public static int CompareByWeightAsc(HttpQualityValue x, HttpQualityValue y)
    {
        return x.CompareTo(y);
    }

    /// <summary>
    /// Compares two QValues in descending order.
    /// </summary>
    /// <param name="x">The first QValue</param>
    /// <param name="y">The second QValue</param>
    /// <returns></returns>
    public static int CompareByWeightDesc(HttpQualityValue x, HttpQualityValue y)
    {
        return y.CompareTo(x);
    }

    #endregion

}