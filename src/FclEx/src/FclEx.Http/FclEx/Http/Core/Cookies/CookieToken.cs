namespace FclEx.Http.Core.Cookies
{
    internal enum CookieToken
    {

        // state types

        Nothing,
        NameValuePair,  // X=Y
        Attribute,      // X
        EndToken,       // ';'
        EndCookie,      // ','
        End,            // EOLN
        Equals,

        // value types

        Comment,
        CommentUrl,
        CookieName,
        Discard,
        Domain,
        Expires,
        MaxAge,
        Path,
        Port,
        Secure,
        HttpOnly,
        Unknown,
        Version
    }
}
