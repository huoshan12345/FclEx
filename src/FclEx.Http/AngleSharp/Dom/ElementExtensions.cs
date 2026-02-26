namespace AngleSharp.Dom;

public static class ElementExtensions
{
    public static HtmlAnchor? GetAnchor(this IElement? element, string? selector = null)
    {
        var a = selector == null
            ? element
            : element?.QuerySelector(selector);

        return a is IHtmlAnchorElement link
            ? new HtmlAnchor(link)
            : null;
    }

    public static string? Href(this IElement? element) => element?.GetAttribute("href");
    public static string? Type(this IElement? element) => element?.GetAttribute("type");
    public static string? Value(this IElement? element) => element?.GetAttribute("value");
    public static string? Title(this IElement? element) => element?.GetAttribute("title");

    public static FormData? GetFormData(this IElement? element, string formSelector, Uri? uri)
    {
        if (element?.QuerySelector(formSelector) is not { } form)
            return null;

        if (form.GetAttribute("action") is not { } action)
            return null;


        var info = new FormData(new Uri(action, UriKind.RelativeOrAbsolute));

        if (!info.SubmitUri.IsAbsoluteUri)
        {
            var baseUri = uri ?? (element.BaseUrl is { } u ? (Uri)u : null);
            if (baseUri != null)
            {
                info.SubmitUri = new Uri(baseUri, info.SubmitUri);
            }
        }

        foreach (var input in form.QuerySelectorAll("input").OfType<IHtmlInputElement>().Where(m => m.Type == "hidden"))
        {
            var name = input.GetAttribute("name");
            if (name.IsNullOrEmpty())
                continue;

            info.Params[name] = input.GetAttribute("value");
        }

        return info;
    }

    [return: NotNullIfNotNull(nameof(element))]
    public static IElement? RemoveJsCss(this IElement? element)
    {
        if (element == null)
            return null;

        foreach (var node in element.QuerySelectorAll("script, style"))
            node.Remove();

        return element;
    }

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

    public static OperationResult<(IElement Element, T Data)> QueryData<T>(this IElement? root, string? selector, Func<IElement, T> func)
    {
        return root.QueryData([selector], func);
    }

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

    public static OperationResult<(IElement Element, string Text)> QueryOwnText(this IElement? root, string? selector, bool trim = true, bool ensureValueIsNotEmpty = true)
    {
        return root.QueryOwnText([selector], trim, ensureValueIsNotEmpty);
    }

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

    public static OperationResult<(IElement Element, string Attribute)> QueryAttribute(this IElement? root, string? selector, string attribute, bool ensureValueIsNotEmpty = true)
    {
        return root.QueryAttribute([selector], attribute, ensureValueIsNotEmpty);
    }

    public static OperationResult<(IElement Element, UriCreator Href)> QueryHref(this IElement? root, string?[] selectors, Uri? baseUri = null)
    {
        var element = root.QueryAttribute(selectors, "href");
        if (element.IsError)
            return element.Exception;

        var (e, href) = element.Value;
        var u = baseUri is null
            ? new Uri(href, UriKind.RelativeOrAbsolute)
            : new Uri(baseUri, href);
        var uriCreator = new UriCreator(u);
        return (e, uriCreator);
    }

    public static OperationResult<(IElement Element, UriCreator Href)> QueryHref(this IElement? root, string? selector, Uri? baseUri = null)
    {
        return root.QueryHref([selector], baseUri);
    }

    public static OperationResult<string> QueryId(this IElement? root, string prefix)
    {
        return root.QueryAttribute($"*[id^='{prefix}']", "id").MapValue(m => m.Attribute.SkipUntil(prefix));
    }

    public static string OwnText(this IElement element)
    {
        using var disposable = StringBuilderHelper.GetCached();
        var builder = disposable.Value;

        foreach (var node in element.ChildNodes.OfType<IText>())
        {
            builder.Append(node.Data);
        }
        return builder.ToString();
    }
}