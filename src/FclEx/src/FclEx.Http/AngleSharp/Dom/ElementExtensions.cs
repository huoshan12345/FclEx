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

    public static SubmitInfo? GetFormSubmitInfo(this IElement? element, string formSelector, Uri? htmlUrl)
    {
        if (element?.QuerySelector(formSelector) is not { } form)
            return null;

        if (form.GetAttribute("action") is not { } action)
            return null;


        var info = new SubmitInfo(new Uri(action, UriKind.RelativeOrAbsolute));

        if (!info.SubmitUrl.IsAbsoluteUri)
        {
            var baseUri = htmlUrl ?? (element.BaseUrl is { } u ? (Uri)u : null);
            if (baseUri != null)
            {
                info.SubmitUrl = new Uri(baseUri, info.SubmitUrl);
            }
        }

        foreach (var input in form.QuerySelectorAll("input").OfType<IHtmlInputElement>().Where(m => m.Type == "hidden"))
        {
            var name = input.GetAttribute("name");
            if (name.IsNullOrEmpty())
                continue;
            info.Paras[name] = input.GetAttribute("value");
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