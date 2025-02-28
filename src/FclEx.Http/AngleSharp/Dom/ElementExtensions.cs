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
}