namespace AngleSharp.Dom;

/// <summary>
/// Extensions for extracting links, forms, attributes, and typed query results from AngleSharp elements.
/// Query helpers return <see cref="OperationResult{T}"/> so missing elements or empty values can be handled without exceptions.
/// </summary>
public static class ElementExtensions
{
    /// <summary>
    /// Returns an anchor wrapper for the current element or for the first descendant matching a selector.
    /// The method returns <see langword="null"/> when the selected element is not an anchor.
    /// </summary>
    public static HtmlAnchor? GetAnchor(this IElement? element, string? selector = null)
    {
        var a = selector == null
            ? element
            : element?.QuerySelector(selector);

        return a is IHtmlAnchorElement link
            ? new HtmlAnchor(link)
            : null;
    }

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
    /// Extracts submit URI, method, and successful form-control values from the first form matching <paramref name="formSelector"/>.
    /// Disabled controls, disabled fieldsets, file/button/reset/submit/image inputs, and unchecked checkbox or radio inputs are skipped.
    /// </summary>
    /// <param name="element">Root element used to search for the form.</param>
    /// <param name="formSelector">CSS selector for the form element.</param>
    /// <param name="uri">Optional base URI used to resolve a relative form action. When omitted, the element's AngleSharp base URL is used.</param>
    public static FormData? GetFormData(this IElement? element, string formSelector, Uri? uri)
    {
        if (element?.QuerySelector(formSelector) is not { } form)
            return null;

        var baseUri = GetBaseUri(element, uri);
        var action = form.GetAttribute("action");
        var submitUri = string.IsNullOrEmpty(action)
            ? baseUri
            : new Uri(action, UriKind.RelativeOrAbsolute);
        if (submitUri == null)
            return null;

        var info = new FormData(submitUri)
        {
            Method = GetFormMethod(form)
        };

        if (!info.SubmitUri.IsAbsoluteUri)
        {
            if (baseUri != null)
            {
                info.SubmitUri = new Uri(baseUri, info.SubmitUri);
            }
        }

        foreach (var control in form.QuerySelectorAll("input, select, textarea"))
        {
            if (!IsSuccessfulControl(control))
                continue;

            AddControlValue(info.Params, control);
        }

        return info;
    }

    private static HttpMethod GetFormMethod(IElement form)
    {
        var method = form.GetAttribute("method");
        return string.Equals(method, "post", StringComparison.OrdinalIgnoreCase)
            ? HttpMethod.Post
            : HttpMethod.Get;
    }

    private static Uri? GetBaseUri(IElement element, Uri? uri)
    {
        if (uri != null)
            return uri;

        return element.BaseUrl is { } baseUrl
            ? new Uri(baseUrl.ToString(), UriKind.Absolute)
            : null;
    }

    private static bool IsSuccessfulControl(IElement control)
    {
        var name = control.GetAttribute("name");
        if (name.IsNullOrEmpty())
            return false;

        if (control.HasAttribute("disabled"))
            return false;

        if (control.Ancestors<IElement>().Any(fieldset => IsDisabledFieldSetAncestor(fieldset, control)))
            return false;

        if (control is IHtmlInputElement)
        {
            var type = GetInputType(control);
            if (type is "button" or "submit" or "reset" or "image" or "file")
                return false;

            if (type is "checkbox" or "radio")
                return control.HasAttribute("checked");
        }

        return true;
    }

    private static bool IsDisabledFieldSetAncestor(IElement fieldset, IElement control)
    {
        if (fieldset.LocalName.EqualsIgnoreCase("fieldset") == false || fieldset.HasAttribute("disabled") == false)
            return false;

        var firstLegend = fieldset.Children.FirstOrDefault(m => m.LocalName.EqualsIgnoreCase("legend"));
        return firstLegend == null || control.Ancestors<IElement>().Contains(firstLegend) == false;
    }

    private static void AddControlValue(UriParams parameters, IElement control)
    {
        var name = control.GetAttribute("name");
        if (name.IsNullOrEmpty())
            return;

        if (control is IHtmlInputElement)
        {
            parameters.Add(name, GetInputValue(control));
            return;
        }

        if (control is IHtmlTextAreaElement)
        {
            parameters.Add(name, control.TextContent);
            return;
        }

        if (control is IHtmlSelectElement)
        {
            AddSelectValue(parameters, name, control);
        }
    }

    private static string? GetInputValue(IElement input)
    {
        var type = GetInputType(input);
        var value = input.GetAttribute("value");
        return type is "checkbox" or "radio"
            ? string.IsNullOrEmpty(value) ? "on" : value
            : value ?? "";
    }

    private static string GetInputType(IElement input)
    {
        var rawType = input.GetAttribute("type");
        return string.IsNullOrEmpty(rawType)
            ? "text"
            : rawType!.ToLowerInvariant();
    }

    private static void AddSelectValue(UriParams parameters, string name, IElement select)
    {
        var options = select.QuerySelectorAll("option").ToArray();
        var selectedOptions = options
            .Where(m => m.HasAttribute("selected"))
            .ToArray();
        var enabledSelectedOptions = selectedOptions
            .Where(IsEnabledOption)
            .ToArray();

        if (select.HasAttribute("multiple") == false && selectedOptions.Length == 0)
        {
            enabledSelectedOptions = options.Where(IsEnabledOption).Take(1).ToArray();
        }

        foreach (var option in enabledSelectedOptions)
        {
            parameters.Add(name, option.GetAttribute("value") ?? option.TextContent);
        }
    }

    private static bool IsEnabledOption(IElement option)
    {
        return option.HasAttribute("disabled") == false
               && option.Ancestors<IElement>().Any(m => m.LocalName.EqualsIgnoreCase("optgroup") && m.HasAttribute("disabled")) == false;
    }

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

    /// <summary>
    /// Returns text from direct child text nodes only.
    /// Text inside descendant elements is intentionally excluded.
    /// </summary>
    public static string OwnText(this IElement element)
    {
        using var disposable = StringBuilder.GetCached();
        var builder = disposable.Value;

        foreach (var node in element.ChildNodes.OfType<IText>())
        {
            builder.Append(node.Data);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Reads the URL target from a meta refresh tag under the element.
    /// The returned value is the raw text after <c>url=</c>, with surrounding quotes and spaces removed.
    /// </summary>
    public static string? GetMetaRefreshUrl(this IElement element)
    {
        var metaTag = element.QuerySelectorAll("meta")
            .FirstOrDefault(m => string.Equals(m.GetAttribute("http-equiv"), "refresh", StringComparison.OrdinalIgnoreCase));
        if (metaTag is null)
            return null;

        var content = metaTag.GetAttribute("content");
        return content == null
            ? null
            : ExtractUrlFromContent(content);

        static string? ExtractUrlFromContent(string content)
        {
            const string urlKey = "url=";
            var urlIndex = content.IndexOf(urlKey, StringComparison.OrdinalIgnoreCase);

            if (urlIndex < 0)
                return null;

            // Extract everything after "url="
            var redirectUrl = content[(urlIndex + urlKey.Length)..];

            // Remove trailing quotes if the HTML contained them (e.g., URL='...')
            return redirectUrl.Trim('\'', '"', ' ');

        }
    }
}
