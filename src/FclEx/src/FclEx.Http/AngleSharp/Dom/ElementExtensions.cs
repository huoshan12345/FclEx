namespace AngleSharp.Dom;

public static class ElementExtensions
{
    public static (Uri Uri, NameValueCollection Query, string text) GetUri(this IElement e, Uri? baseUri = null, Action<NameValueCollection>? query = null)
    {
        var url = e.GetAttribute("href");
        if (url.IsNullOrEmpty())
            throw new InvalidOperationException("The is no href attribute in the element");


        var text = e.TextContent;
        var uri = new Uri(url, UriKind.RelativeOrAbsolute);
        if (!uri.IsAbsoluteUri && baseUri == null)
            throw new InvalidOperationException("Base uri cannot be null when href is a relative url");

        var uriCreator = new UriCreator(uri.IsAbsoluteUri ? uri : new Uri(baseUri!, uri));
        query?.Invoke(uriCreator.QueryValues);
        var u = uriCreator.GetUri();
        return (u, uriCreator.QueryValues, text);
    }

    public static HtmlAnchor GetAnchor(this IElement? e, string? selector = null)
    {
        var a = selector == null ? e : e?.QuerySelector(selector);
        return a is IHtmlAnchorElement link
            ? new HtmlAnchor(link)
            : HtmlAnchor.Empty;
    }

    public static string? Href(this IElement? e) => e?.GetAttribute("href");
    public static string? Type(this IElement? e) => e?.GetAttribute("type");
    public static string? Value(this IElement? e) => e?.GetAttribute("value");
    public static string? Title(this IElement? e) => e?.GetAttribute("title");

    public static SubmitInfo? GetFormSubmitInfo(this IElement? e, string formSelector, Uri? htmlUrl)
    {
        if (e?.QuerySelector(formSelector) is not { } form)
            return null;

        if (form.GetAttribute("action") is not { } action)
            return null;


        var info = new SubmitInfo(new Uri(action, UriKind.RelativeOrAbsolute));

        if (!info.SubmitUrl.IsAbsoluteUri)
        {
            var baseUri = htmlUrl ?? (e.BaseUrl is { } u ? (Uri)u : null);
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

    public static IElement? RemoveJsCss(this IElement? e)
    {
        if (e == null)
            return null;

        foreach (var node in e.QuerySelectorAll("script, style"))
            node.Remove();

        return e;
    }
}