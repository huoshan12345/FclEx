using System;
using System.Globalization;
using System.Net;
using FclEx.Helpers;

namespace FclEx.Http.Core.Cookies
{
    internal class CookieParser
    {
        private static readonly string[] _dateTimeFormats =
        {
            "ddd, d MMM yyyy HH:mm:ss Z",
            "ddd, d-MMM-yyyy HH:mm:ss Z",
            "ddd, d-MMM-yy HH:mm:ss Z",
        };
        // fields

        readonly CookieTokenizer m_tokenizer;
        CookieInternal m_savedCookie;

        // constructors

        internal CookieParser(string cookieString)
        {
            m_tokenizer = new CookieTokenizer(cookieString);
        }

        // properties

        // methods

        //
        // Get
        //
        //  Gets the next cookie
        //
        // Inputs:
        //  Nothing
        //
        // Outputs:
        //  Nothing
        //
        // Assumes:
        //  Nothing
        //
        // Returns:
        //  new cookie object, or null if there's no more
        //
        // Throws:
        //  Nothing
        //

        internal CookieInternal Get()
        {

            CookieInternal cookie = null;

            // only first ocurence of an attribute value must be counted
            bool commentSet = false;
            bool commentUriSet = false;
            bool domainSet = false;
            bool expiresSet = false;
            bool pathSet = false;
            bool portSet = false; //special case as it may have no value in header
            bool versionSet = false;
            bool secureSet = false;
            bool discardSet = false;
            bool rejected = false;

            do
            {
                CookieToken token = m_tokenizer.Next(cookie == null, true);
                if (cookie == null && (token == CookieToken.NameValuePair || token == CookieToken.Attribute))
                {
                    cookie = new CookieInternal();
                    if (cookie.InternalSetName(m_tokenizer.Name) == false)
                    {
                        //will be rejected
                        cookie.InternalSetName(string.Empty);
                        rejected = true;
                    }
                    cookie.Value = m_tokenizer.Value;
                }
                else
                {
                    switch (token)
                    {
                        case CookieToken.NameValuePair:
                            switch (m_tokenizer.Token)
                            {
                                case CookieToken.Comment:
                                {
                                    if (!commentSet)
                                    {
                                        commentSet = true;
                                        cookie.Comment = m_tokenizer.Value;
                                    }
                                    break;
                                }
                                case CookieToken.CommentUrl:
                                {
                                    if (!commentUriSet)
                                    {
                                        commentUriSet = true;
                                        if (Uri.TryCreate(CheckQuoted(m_tokenizer.Value), UriKind.Absolute,
                                            out var parsed))
                                        {
                                            cookie.CommentUri = parsed;
                                        }
                                    }

                                    break;
                                }
                                case CookieToken.Domain:
                                {
                                    if (!domainSet)
                                    {
                                        domainSet = true;
                                        cookie.Domain = CheckQuoted(m_tokenizer.Value);
                                        cookie.IsQuotedDomain = m_tokenizer.Quoted;
                                    }

                                    break;
                                }
                                case CookieToken.Expires:
                                {
                                    if (!expiresSet)
                                    {
                                        expiresSet = true;
                                        const DateTimeStyles style = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal;
                                        var unQuotedValue = CheckQuoted(m_tokenizer.Value);

                                        if (DateTime.TryParse(unQuotedValue,
                                            CultureInfo.InvariantCulture, style, out var expires))
                                        {
                                            cookie.Expires = expires;
                                        }
                                        else if (DateTime.TryParseExact(unQuotedValue, _dateTimeFormats,
                                            DateTimeCultureInfo.TwoDigitYear, style, out expires))
                                        {
                                            cookie.Expires = expires;
                                        }
                                        else
                                        {
                                            //this cookie will be rejected
                                            cookie.InternalSetName(string.Empty);
                                            rejected = true;
                                        }
                                    }
                                    break;
                                }
                                case CookieToken.MaxAge:
                                {
                                    if (!expiresSet)
                                    {
                                        expiresSet = true;
                                        if (int.TryParse(CheckQuoted(m_tokenizer.Value), out var parsed))
                                        {
                                            cookie.Expires = DateTime.Now.AddSeconds((double)parsed);
                                        }
                                        else
                                        {
                                            //this cookie will be rejected
                                            cookie.InternalSetName(string.Empty);
                                            rejected = true;
                                        }
                                    }

                                    break;
                                }
                                case CookieToken.Path:
                                {
                                    if (!pathSet)
                                    {
                                        pathSet = true;
                                        cookie.Path = m_tokenizer.Value;
                                    }

                                    break;
                                }
                                case CookieToken.Port:
                                {
                                    if (!portSet)
                                    {
                                        portSet = true;
                                        try
                                        {
                                            cookie.Port = m_tokenizer.Value;
                                        }
                                        catch
                                        {
                                            //this cookie will be rejected
                                            cookie.InternalSetName(string.Empty);
                                            rejected = true;
                                        }
                                    }

                                    break;
                                }

                                case CookieToken.Version:
                                {
                                    if (!versionSet)
                                    {
                                        versionSet = true;
                                        if (int.TryParse(CheckQuoted(m_tokenizer.Value), out var parsed))
                                        {
                                            cookie.Version = parsed;
                                            cookie.IsQuotedVersion = m_tokenizer.Quoted;
                                        }
                                        else
                                        {
                                            //this cookie will be rejected
                                            cookie.InternalSetName(string.Empty);
                                            rejected = true;
                                        }
                                    }
                                    break;
                                }
                            }
                            break;

                        case CookieToken.Attribute:
                            switch (m_tokenizer.Token)
                            {
                                case CookieToken.Discard:
                                    if (!discardSet)
                                    {
                                        discardSet = true;
                                        cookie.Discard = true;
                                    }
                                    break;

                                case CookieToken.Secure:
                                    if (!secureSet)
                                    {
                                        secureSet = true;
                                        cookie.Secure = true;
                                    }
                                    break;

                                case CookieToken.HttpOnly:
                                    cookie.HttpOnly = true;
                                    break;

                                case CookieToken.Port:
                                    if (!portSet)
                                    {
                                        portSet = true;
                                        cookie.Port = string.Empty;
                                    }
                                    break;
                            }
                            break;
                    }
                }
            } while (!m_tokenizer.Eof && !m_tokenizer.EndOfCookie && !rejected);
            return cookie;
        }

        // twin parsing method, different enough that it's better to split it into
        // a different method
        internal CookieInternal GetServer()
        {

            CookieInternal cookie = m_savedCookie;
            m_savedCookie = null;

            // only first ocurence of an attribute value must be counted
            bool domainSet = false;
            bool pathSet = false;
            bool portSet = false; //special case as it may have no value in header

            do
            {
                bool first = cookie == null || cookie.Name == null || cookie.Name.Length == 0;
                CookieToken token = m_tokenizer.Next(first, false);

                if (first && (token == CookieToken.NameValuePair || token == CookieToken.Attribute))
                {
                    if (cookie == null)
                    {
                        cookie = new CookieInternal();
                    }
                    if (cookie.InternalSetName(m_tokenizer.Name) == false)
                    {
                        //will be rejected
                        cookie.InternalSetName(string.Empty);
                    }
                    cookie.Value = m_tokenizer.Value;
                }
                else
                {
                    switch (token)
                    {
                        case CookieToken.NameValuePair:
                            switch (m_tokenizer.Token)
                            {
                                case CookieToken.Domain:
                                    if (!domainSet)
                                    {
                                        domainSet = true;
                                        cookie.Domain = CheckQuoted(m_tokenizer.Value);
                                        cookie.IsQuotedDomain = m_tokenizer.Quoted;
                                    }
                                    break;

                                case CookieToken.Path:
                                    if (!pathSet)
                                    {
                                        pathSet = true;
                                        cookie.Path = m_tokenizer.Value;
                                    }
                                    break;

                                case CookieToken.Port:
                                    if (!portSet)
                                    {
                                        portSet = true;
                                        try
                                        {
                                            cookie.Port = m_tokenizer.Value;
                                        }
                                        catch (CookieException)
                                        {
                                            //this cookie will be rejected
                                            cookie.InternalSetName(string.Empty);
                                        }
                                    }
                                    break;

                                case CookieToken.Version:
                                    // this is a new cookie, this token is for the next cookie.
                                    m_savedCookie = new CookieInternal();
                                    int parsed;
                                    if (int.TryParse(m_tokenizer.Value, out parsed))
                                    {
                                        m_savedCookie.Version = parsed;
                                    }
                                    return cookie;

                                case CookieToken.Unknown:
                                    // this is a new cookie, the token is for the next cookie.
                                    m_savedCookie = new CookieInternal();
                                    if (m_savedCookie.InternalSetName(m_tokenizer.Name) == false)
                                    {
                                        //will be rejected
                                        m_savedCookie.InternalSetName(string.Empty);
                                    }
                                    m_savedCookie.Value = m_tokenizer.Value;
                                    return cookie;

                            }
                            break;

                        case CookieToken.Attribute:
                            switch (m_tokenizer.Token)
                            {
                                case CookieToken.Port:
                                    if (!portSet)
                                    {
                                        portSet = true;
                                        cookie.Port = string.Empty;
                                    }
                                    break;
                            }
                            break;
                    }
                }
            } while (!m_tokenizer.Eof && !m_tokenizer.EndOfCookie);
            return cookie;
        }

        internal static string CheckQuoted(string value)
        {
            if (value.Length < 2 || value[0] != '\"' || value[value.Length - 1] != '\"')
                return value;

            return value.Length == 2 ? string.Empty : value.Substring(1, value.Length - 2);
        }
    }

}
