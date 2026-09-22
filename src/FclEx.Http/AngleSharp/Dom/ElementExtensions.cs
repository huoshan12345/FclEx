namespace AngleSharp.Dom;

/// <summary>
/// Extensions for extracting attributes and typed query results from AngleSharp elements.
/// Query helpers return <see cref="OperationResult{T}"/> so missing elements or empty values can be handled without exceptions.
/// </summary>
public static class ElementExtensions
{
    /// <summary>
    /// Returns the element's raw <c>href</c> attribute value.
    /// </summary>
    public static string? Href(this IElement? element) => element?.GetAttribute("href");

    /// <summary>
    /// Returns the element's raw <c>type</c> attribute value.
    /// </summary>
    public static string? Type(this IElement? element) => element?.GetAttribute("type");

    /// <summary>
    /// Returns the element's raw <c>value</c> attribute value.
    /// </summary>
    public static string? Value(this IElement? element) => element?.GetAttribute("value");

    /// <summary>
    /// Returns the element's raw <c>title</c> attribute value.
    /// </summary>
    public static string? Title(this IElement? element) => element?.GetAttribute("title");

    /// <summary>
    /// Queries the first element matching any selector and maps it to caller-defined data.
    /// A <see langword="null"/> selector means the root element itself.
    /// </summary>
    public static OperationResult<(IElement Element, T Data)> QueryData<T>(this IElement? root, string?[] selectors, Func<IElement, T> func)
    {
        foreach (var selector in selectors)
        {
            var element = selector is null
                ? root
                : root?.QuerySelector(selector);

            if (element is null)
                continue;

            var value = func(element);
            return (element, value);
        }

        return $"No element found by selectors '{selectors.JoinWith(", ")}'";
    }

    /// <summary>
    /// Queries one selector and maps the matched element to caller-defined data.
    /// A <see langword="null"/> selector means the root element itself.
    /// </summary>
    public static OperationResult<(IElement Element, T Data)> QueryData<T>(this IElement? root, string? selector, Func<IElement, T> func)
    {
        return root.QueryData([selector], func);
    }

    /// <summary>
    /// Queries the first matching element and returns only its direct text-node content.
    /// Child element text is not included.
    /// </summary>
    public static OperationResult<(IElement Element, string Text)> QueryOwnText(this IElement? root, string?[] selectors, bool trim = true, bool ensureValueIsNotEmpty = true)
    {
        var result = root.QueryData(selectors, m => m.OwnText());
        if (result.IsError)
            return result.Exception;

        var (element, value) = result.Value;

        if (trim)
            value = value.Trim();

        if (ensureValueIsNotEmpty && value.IsNullOrEmpty())
            return $"own text is empty in the element by selectors '{selectors.JoinWith(", ")}'";

        return (element, value);
    }

    /// <summary>
    /// Queries one selector and returns only the matched element's direct text-node content.
    /// Child element text is not included.
    /// </summary>
    public static OperationResult<(IElement Element, string Text)> QueryOwnText(this IElement? root, string? selector, bool trim = true, bool ensureValueIsNotEmpty = true)
    {
        return root.QueryOwnText([selector], trim, ensureValueIsNotEmpty);
    }

    /// <summary>
    /// Queries the first matching element and returns a required attribute value.
    /// Missing attributes and, by default, empty values are returned as operation errors.
    /// </summary>
    public static OperationResult<(IElement Element, string Attribute)> QueryAttribute(this IElement? root, string?[] selectors, string attribute, bool ensureValueIsNotEmpty = true)
    {
        var result = root.QueryData(selectors, m => m.GetAttribute(attribute));
        if (result.IsError)
            return result.Exception;

        var (element, value) = result.Value;
        if (value is null)
            return $"No attribute '{attribute}' found in the element by selectors '{selectors.JoinWith(", ")}'";

        if (ensureValueIsNotEmpty && value.IsNullOrEmpty())
            return $"Attribute '{attribute}' is empty in the element by selectors '{selectors.JoinWith(", ")}'";

        return (element, value);
    }

    /// <summary>
    /// Queries one selector and returns a required attribute value.
    /// Missing attributes and, by default, empty values are returned as operation errors.
    /// </summary>
    public static OperationResult<(IElement Element, string Attribute)> QueryAttribute(this IElement? root, string? selector, string attribute, bool ensureValueIsNotEmpty = true)
    {
        return root.QueryAttribute([selector], attribute, ensureValueIsNotEmpty);
    }

    /// <summary>
    /// Queries the first matching element, reads its <c>href</c> attribute, and returns a mutable URI wrapper.
    /// When <paramref name="baseUri"/> is supplied, relative href values are resolved against it.
    /// </summary>
    public static OperationResult<(IElement Element, UriCreator Href)> QueryHref(this IElement? root, string?[] selectors, Uri? baseUri = null)
    {
        var element = root.QueryAttribute(selectors, "href");
        if (element.IsError)
            return element.Exception;

        var (e, href) = element.Value;
        return Operation.Execute(() => Create(e, href, baseUri));

        static (IElement Element, UriCreator Href) Create(IElement e, string href, Uri? baseUri)
        {
            var u = baseUri is null
                ? new Uri(href, UriKind.RelativeOrAbsolute)
                : new Uri(baseUri, href);
            var uriCreator = new UriCreator(u);
            return (e, uriCreator);
        }
    }

    /// <summary>
    /// Queries one selector, reads its <c>href</c> attribute, and returns a mutable URI wrapper.
    /// When <paramref name="baseUri"/> is supplied, relative href values are resolved against it.
    /// </summary>
    public static OperationResult<(IElement Element, UriCreator Href)> QueryHref(this IElement? root, string? selector, Uri? baseUri = null)
    {
        return root.QueryHref([selector], baseUri);
    }

    private static readonly string?[] TopLevelSelectors = [null];

    /// <summary>
    /// Maps the root element itself to caller-defined data.
    /// </summary>
    public static OperationResult<(IElement Element, T Data)> QueryData<T>(this IElement? root, Func<IElement, T> func)
    {
        return root.QueryData(TopLevelSelectors, func);
    }

    /// <summary>
    /// Returns only the root element's direct text-node content.
    /// Child element text is not included.
    /// </summary>
    public static OperationResult<(IElement Element, string Text)> QueryOwnText(this IElement? root, bool trim = true, bool ensureValueIsNotEmpty = true)
    {
        return root.QueryOwnText(TopLevelSelectors, trim, ensureValueIsNotEmpty);
    }

    /// <summary>
    /// Returns a required attribute value from the root element.
    /// Missing attributes and, by default, empty values are returned as operation errors.
    /// </summary>
    public static OperationResult<(IElement Element, string Attribute)> QueryAttribute(this IElement? root, string attribute, bool ensureValueIsNotEmpty = true)
    {
        return root.QueryAttribute(TopLevelSelectors, attribute, ensureValueIsNotEmpty);
    }

    /// <summary>
    /// Reads the root element's <c>href</c> attribute and returns a mutable URI wrapper.
    /// When <paramref name="baseUri"/> is supplied, relative href values are resolved against it.
    /// </summary>
    public static OperationResult<(IElement Element, UriCreator Href)> QueryHref(this IElement? root, Uri? baseUri = null)
    {
        return root.QueryHref(TopLevelSelectors, baseUri);
    }

    /// <summary>
    /// Finds the first element whose id starts with <paramref name="prefix"/> and returns the part after the prefix.
    /// The prefix is escaped before being embedded in the CSS attribute selector.
    /// </summary>
    public static OperationResult<string> QueryId(this IElement? root, string prefix)
    {
        return root.QueryAttribute($"*[id^='{EscapeCssString(prefix)}']", "id").MapValue(m => m.Attribute.SkipUntil(prefix));
    }

    private static string EscapeCssString(string value)
    {
        using var disposable = StringBuilder.GetCached();
        var builder = disposable.Value;

        foreach (var c in value)
        {
            switch (c)
            {
                case '\\':
                case '\'':
                    builder.Append('\\');
                    builder.Append(c);
                    break;
                case '\r':
                    builder.Append("\\D ");
                    break;
                case '\n':
                    builder.Append("\\A ");
                    break;
                case '\f':
                    builder.Append("\\C ");
                    break;
                case '\t':
                    builder.Append("\\9 ");
                    break;
                default:
                    if (char.IsControl(c))
                    {
                        builder.Append('\\');
                        builder.Append(((int)c).ToString("X", CultureInfo.InvariantCulture));
                        builder.Append(' ');
                    }
                    else
                    {
                        builder.Append(c);
                    }
                    break;
            }
        }

        return builder.ToString();
    }
}
