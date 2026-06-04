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

        if (control.Ancestors<IElement>().Any(m => m.LocalName.EqualsIgnoreCase("fieldset") && m.HasAttribute("disabled")))
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
            .Where(m => m.HasAttribute("selected") && m.HasAttribute("disabled") == false)
            .ToArray();

        if (select.HasAttribute("multiple") == false && selectedOptions.Length == 0)
        {
            selectedOptions = options.Where(m => m.HasAttribute("disabled") == false).Take(1).ToArray();
        }

        foreach (var option in selectedOptions)
        {
            parameters.Add(name, option.GetAttribute("value") ?? option.TextContent);
        }
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
