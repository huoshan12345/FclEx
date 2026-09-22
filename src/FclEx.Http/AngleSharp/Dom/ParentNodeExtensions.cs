namespace AngleSharp.Dom;

/// <summary>
/// Extensions for querying and mutating AngleSharp parent nodes.
/// </summary>
public static class ParentNodeExtensions
{
    /// <summary>
    /// Removes all descendant <c>script</c> and <c>style</c> elements from a node and returns the same node instance.
    /// A <see langword="null"/> input returns <see langword="null"/>.
    /// </summary>
    [return: NotNullIfNotNull(nameof(node))]
    public static T? RemoveJsCss<T>(this T? node) where T : IParentNode
    {
        if (node == null)
            return default;

        foreach (var childNode in node.QuerySelectorAll("script, style"))
        {
            childNode.Remove();
        }
        return node;
    }

    /// <summary>
    /// Returns an anchor wrapper for the current element or for the first descendant matching a selector.
    /// The method returns <see langword="null"/> when the selected element is not an anchor.
    /// </summary>
    public static HtmlAnchor? GetAnchor(this IParentNode? element, string? selector = null)
    {
        var a = selector == null
            ? element
            : element?.QuerySelector(selector);

        return a is IHtmlAnchorElement link
            ? new HtmlAnchor(link)
            : null;
    }

    /// <summary>
    /// Extracts submit URI, method, and successful form-control values from the first form matching <paramref name="formSelector"/>.
    /// Disabled controls, disabled fieldsets, file/button/reset/submit/image inputs, and unchecked checkbox or radio inputs are skipped.
    /// </summary>
    /// <param name="element">Root element used to search for the form.</param>
    /// <param name="formSelector">CSS selector for the form element.</param>
    /// <param name="uri">Optional base URI used to resolve a relative form action. When omitted, the element's AngleSharp base URL is used.</param>
    public static FormData? GetFormData<T>(this T? element, string formSelector, Uri? uri) where T : INode, IParentNode
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

    private static Uri? GetBaseUri(INode node, Uri? uri)
    {
        if (uri != null)
            return uri;

        return node.BaseUrl is { } baseUrl
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
    /// Reads the URL target from a meta refresh tag under the element.
    /// The returned value is the raw text after <c>url=</c>, with surrounding quotes and spaces removed.
    /// </summary>
    public static string? GetMetaRefreshUrl(this IParentNode element)
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
