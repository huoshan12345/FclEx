namespace FclEx.Extensions;

public static class CookieExtensions
{
    public static Cookie Clone(this Cookie source)
    {
        var newCookie = new Cookie(source.Name, source.Value)
        {
            Comment = source.Comment,
            CommentUri = source.CommentUri,
            Discard = source.Discard,
            Domain = source.Domain,
            Expired = source.Expired,
            Expires = source.Expires,
            HttpOnly = source.HttpOnly,
            Path = source.Path,
            Port = source.Port,
            Secure = source.Secure,
            Version = source.Version,
            // Note: DomainImplicit and PathImplicit are internal/private set
            // and handled by the CookieContainer logic.
        };
        return newCookie;
    }
}
