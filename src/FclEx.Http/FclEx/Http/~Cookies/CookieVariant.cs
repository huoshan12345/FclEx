namespace FclEx.Http;

internal enum CookieVariant
{
    Unknown,
    Plain,
    Rfc2109,
    Rfc2965,
    Default = Rfc2109
}