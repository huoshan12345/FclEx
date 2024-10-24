using System.Collections;
using FclEx.Comparers;

namespace FclEx.Http;

/// <summary>
/// Provides a collection for working with qvalue http headers 
/// </summary>
/// <remarks>
/// accept-encoding spec: 
///		http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html
/// </remarks>
[DebuggerDisplay("QValue[{Count}, {AcceptWildcard}]")]
public sealed class HttpQualityValueList : IEnumerable<HttpQualityValue>
{
    private static readonly char[] _delimiters = { ',' };
    private static readonly IComparer<float> _comparer =
        MemberComparerBuilder<float>
            .Create(m => m, true)
            .Build();
    private readonly SortedDictionary<float, List<HttpQualityValue>> _dic = new(_comparer);

    #region Fields

    public int Count { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new instance of an QValueList list from 
    /// the given string of comma delimited values
    /// </summary>
    /// <param name="values">The raw string of qvalues to load</param>
    public HttpQualityValueList(string values)
        : this(null == values ? Array.Empty<string>() : values.Split(_delimiters, StringSplitOptions.RemoveEmptyEntries))
    {

    }

    /// <summary>
    /// Creates a new instance of an QValueList from 
    /// the given string array of qvalues
    /// </summary>
    /// <param name="values">The array of qvalue strings 
    /// i.e. name(;q=[0-9\.]+)?</param>
    /// <remarks>
    /// Should AcceptWildcard include */* as well? 
    /// What about other wildcard forms?
    /// </remarks>
    public HttpQualityValueList(IEnumerable<string> values)
    {
        foreach (var value in values)
        {
            var qvalue = HttpQualityValue.Parse(value.Trim());
            if (qvalue.Name.Equals("*")) // wildcard
                AcceptWildcard = qvalue.CanAccept;
            Add(qvalue);
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Whether or not the wildcarded encoding is available and allowed
    /// </summary>
    public bool AcceptWildcard { get; }

    /// <summary>
    /// Synonym for FindPreferred
    /// </summary>
    /// <param name="candidates">The preferred order in which to return an encoding</param>
    /// <returns>An QValue based on weight, or null</returns>
    public HttpQualityValue this[params string[] candidates] => FindPreferred(candidates);

    #endregion

    /// <summary>
    /// Adds an item to the list, then applies sorting 
    /// if AutoSort is enabled.
    /// </summary>
    /// <param name="item">The item to add</param>
    public void Add(HttpQualityValue item)
    {
        if (!_dic.TryGetValue(item.Weight, out var list))
        {
            list = new List<HttpQualityValue>();
            _dic[item.Weight] = list;
        }
        list.Add(item);
        Count++;
    }

    public HttpQualityValue Find(Func<HttpQualityValue, bool> predicate) => this.FirstOrDefault(predicate);

    /// <summary>
    /// Finds the first QValue with the given name (case-insensitive)
    /// </summary>
    /// <param name="name">The name of the QValue to search for</param>
    /// <returns></returns>
    public HttpQualityValue Find(string name)
    {
        return Find(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns the first match found from the given candidates that is accepted
    /// </summary>
    /// <param name="candidates">The list of names to find</param>
    /// <returns>The first QValue match to be found</returns>
    /// <remarks>Loops from the first item in the list to the last and finds the 
    /// first candidate that can be accepted - the list must be sorted for weight 
    /// prior to calling this method.</remarks>
    public HttpQualityValue FindPreferred(params string[] candidates)
    {
        if (_dic.TryGetFirst(out var pair) && !candidates.IsNullOrEmpty())
        {
            var list = pair.Value;
            foreach (var candidate in candidates)
            {
                if (AcceptWildcard)
                    return new HttpQualityValue(candidate);

                if (list.Where(m => candidate.Equals(m.Name, StringComparison.OrdinalIgnoreCase) && m.CanAccept).TryGetFirst(out var item))
                    return item;
            }
        }
        return default;
    }

    public IEnumerator<HttpQualityValue> GetEnumerator()
    {
        foreach (var (_, list) in _dic)
        {
            foreach (var item in list)
            {
                yield return item;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}