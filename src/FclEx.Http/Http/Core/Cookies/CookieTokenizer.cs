using System;

namespace FclEx.Http.Core.Cookies
{
    //
    // CookieTokenizer
    //
    //  Used to split a single or multi-cookie (header) string into individual
    //  tokens
    //

    internal class CookieTokenizer
    {

        // fields

        bool m_eofCookie;
        int m_index;
        int m_length;
        string m_name;
        bool m_quoted;
        int m_start;
        CookieToken m_token;
        int m_tokenLength;
        string m_tokenStream;
        string m_value;

        // constructors

        internal CookieTokenizer(string tokenStream)
        {
            m_length = tokenStream.Length;
            m_tokenStream = tokenStream;
        }

        // properties

        internal bool EndOfCookie
        {
            get
            {
                return m_eofCookie;
            }
            set
            {
                m_eofCookie = value;
            }
        }

        internal bool Eof
        {
            get
            {
                return m_index >= m_length;
            }
        }

        internal string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        internal bool Quoted
        {
            get
            {
                return m_quoted;
            }
            set
            {
                m_quoted = value;
            }
        }

        internal CookieToken Token
        {
            get
            {
                return m_token;
            }
            set
            {
                m_token = value;
            }
        }

        internal string Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        // methods

        //
        // Extract
        //
        //  extract the current token
        //

        internal string Extract()
        {

            string tokenString = string.Empty;

            if (m_tokenLength != 0)
            {
                tokenString = m_tokenStream.Substring(m_start, m_tokenLength);
                if (!Quoted)
                {
                    tokenString = tokenString.Trim();
                }
            }
            return tokenString;
        }

        //
        // FindNext
        //
        //  Find the start and length of the next token. The token is terminated
        //  by one of:
        //
        //      - end-of-line
        //      - end-of-cookie: unquoted comma separates multiple cookies
        //      - end-of-token: unquoted semi-colon
        //      - end-of-name: unquoted equals
        //
        // Inputs:
        //  <argument>  ignoreComma
        //      true if parsing doesn't stop at a comma. This is only true when
        //      we know we're parsing an original cookie that has an expires=
        //      attribute, because the format of the time/date used in expires
        //      is:
        //          Wdy, dd-mmm-yyyy HH:MM:SS GMT
        //
        //  <argument>  ignoreEquals
        //      true if parsing doesn't stop at an equals sign. The LHS of the
        //      first equals sign is an attribute name. The next token may
        //      include one or more equals signs. E.g.,
        //
        //          SESSIONID=ID=MSNx45&q=33
        //
        // Outputs:
        //  <member>    m_index
        //      incremented to the last position in m_tokenStream contained by
        //      the current token
        //
        //  <member>    m_start
        //      incremented to the start of the current token
        //
        //  <member>    m_tokenLength
        //      set to the length of the current token
        //
        // Assumes:
        //  Nothing
        //
        // Returns:
        //  type of CookieToken found:
        //
        //      End         - end of the cookie string
        //      EndCookie   - end of current cookie in (potentially) a
        //                    multi-cookie string
        //      EndToken    - end of name=value pair, or end of an attribute
        //      Equals      - end of name=
        //
        // Throws:
        //  Nothing
        //

        internal CookieToken FindNext(bool ignoreComma, bool ignoreEquals)
        {
            m_tokenLength = 0;
            m_start = m_index;
            while ((m_index < m_length) && Char.IsWhiteSpace(m_tokenStream[m_index]))
            {
                ++m_index;
                ++m_start;
            }

            CookieToken token = CookieToken.End;
            int increment = 1;

            if (!Eof)
            {
                if (m_tokenStream[m_index] == '"')
                {
                    Quoted = true;
                    ++m_index;
                    bool quoteOn = false;
                    while (m_index < m_length)
                    {
                        char currChar = m_tokenStream[m_index];
                        if (!quoteOn && currChar == '"')
                            break;
                        if (quoteOn)
                            quoteOn = false;
                        else if (currChar == '\\')
                            quoteOn = true;
                        ++m_index;
                    }
                    if (m_index < m_length)
                    {
                        ++m_index;
                    }
                    m_tokenLength = m_index - m_start;
                    increment = 0;
                    // if we are here, reset ignoreComma
                    // In effect, we ignore everything after quoted string till next delimiter
                    ignoreComma = false;
                }
                while ((m_index < m_length)
                       && (m_tokenStream[m_index] != ';')
                       && (ignoreEquals || (m_tokenStream[m_index] != '='))
                       && (ignoreComma || (m_tokenStream[m_index] != ',')))
                {

                    // Fixing 2 things:
                    // 1) ignore day of week in cookie string
                    // 2) revert ignoreComma once meet it, so won't miss the next cookie)
                    if (m_tokenStream[m_index] == ',')
                    {
                        // do not ignore day of week in cookie string
                        //m_start = m_index + 1;
                        //m_tokenLength = -1;
                        ignoreComma = false;
                    }
                    ++m_index;
                    m_tokenLength += increment;

                }
                if (!Eof)
                {
                    switch (m_tokenStream[m_index])
                    {
                        case ';':
                            token = CookieToken.EndToken;
                            break;

                        case '=':
                            token = CookieToken.Equals;
                            break;

                        default:
                            token = CookieToken.EndCookie;
                            break;
                    }
                    ++m_index;
                }
            }
            return token;
        }

        //
        // Next
        //
        //  Get the next cookie name/value or attribute
        //
        //  Cookies come in the following formats:
        //
        //      1. Version0
        //          Set-Cookie: [<name>][=][<value>]
        //                      [; expires=<date>]
        //                      [; path=<path>]
        //                      [; domain=<domain>]
        //                      [; secure]
        //          Cookie: <name>=<value>
        //
        //          Notes: <name> and/or <value> may be blank
        //                 <date> is the RFC 822/1123 date format that
        //                 incorporates commas, e.g.
        //                 "Wednesday, 09-Nov-99 23:12:40 GMT"
        //
        //      2. RFC 2109
        //          Set-Cookie: 1#{
        //                          <name>=<value>
        //                          [; comment=<comment>]
        //                          [; domain=<domain>]
        //                          [; max-age=<seconds>]
        //                          [; path=<path>]
        //                          [; secure]
        //                          ; Version=<version>
        //                      }
        //          Cookie: $Version=<version>
        //                  1#{
        //                      ; <name>=<value>
        //                      [; path=<path>]
        //                      [; domain=<domain>]
        //                  }
        //
        //      3. RFC 2965
        //          Set-Cookie2: 1#{
        //                          <name>=<value>
        //                          [; comment=<comment>]
        //                          [; commentURL=<comment>]
        //                          [; discard]
        //                          [; domain=<domain>]
        //                          [; max-age=<seconds>]
        //                          [; path=<path>]
        //                          [; ports=<portlist>]
        //                          [; secure]
        //                          ; Version=<version>
        //                       }
        //          Cookie: $Version=<version>
        //                  1#{
        //                      ; <name>=<value>
        //                      [; path=<path>]
        //                      [; domain=<domain>]
        //                      [; port="<port>"]
        //                  }
        //          [Cookie2: $Version=<version>]
        //
        // Inputs:
        //  <argument>  first
        //      true if this is the first name/attribute that we have looked for
        //      in the cookie stream
        //
        // Outputs:
        //
        // Assumes:
        //  Nothing
        //
        // Returns:
        //  type of CookieToken found:
        //
        //      - Attribute
        //          - token was single-value. May be empty. Caller should check
        //            Eof or EndCookie to determine if any more action needs to
        //            be taken
        //
        //      - NameValuePair
        //          - Name and Value are meaningful. Either may be empty
        //
        // Throws:
        //  Nothing
        //

        internal CookieToken Next(bool first, bool parseResponseCookies)
        {

            Reset();

            CookieToken terminator = FindNext(false, false);
            if (terminator == CookieToken.EndCookie)
            {
                EndOfCookie = true;
            }

            if ((terminator == CookieToken.End) || (terminator == CookieToken.EndCookie))
            {
                if ((Name = Extract()).Length != 0)
                {
                    Token = TokenFromName(parseResponseCookies);
                    return CookieToken.Attribute;
                }
                return terminator;
            }
            Name = Extract();
            if (first)
            {
                Token = CookieToken.CookieName;
            }
            else
            {
                Token = TokenFromName(parseResponseCookies);
            }
            if (terminator == CookieToken.Equals)
            {
                terminator = FindNext(!first && (Token == CookieToken.Expires), true);
                if (terminator == CookieToken.EndCookie)
                {
                    EndOfCookie = true;
                }
                Value = Extract();
                return CookieToken.NameValuePair;
            }
            else
            {
                return CookieToken.Attribute;
            }
        }

        //
        // Reset
        //
        //  set up this tokenizer for finding the next name/value pair or
        //  attribute, or end-of-[token, cookie, or line]
        //

        internal void Reset()
        {
            m_eofCookie = false;
            m_name = string.Empty;
            m_quoted = false;
            m_start = m_index;
            m_token = CookieToken.Nothing;
            m_tokenLength = 0;
            m_value = string.Empty;
        }

        private struct RecognizedAttribute
        {

            string m_name;
            CookieToken m_token;

            internal RecognizedAttribute(string name, CookieToken token)
            {
                m_name = name;
                m_token = token;
            }

            internal CookieToken Token
            {
                get
                {
                    return m_token;
                }
            }

            internal bool IsEqualTo(string value)
            {
                return string.Compare(m_name, value, StringComparison.OrdinalIgnoreCase) == 0;
            }
        }

        //
        // recognized attributes in order of expected commonality
        //

        static RecognizedAttribute[] RecognizedAttributes = {
            new RecognizedAttribute(CookieInternal.PathAttributeName, CookieToken.Path),
            new RecognizedAttribute(CookieInternal.MaxAgeAttributeName, CookieToken.MaxAge),
            new RecognizedAttribute(CookieInternal.ExpiresAttributeName, CookieToken.Expires),
            new RecognizedAttribute(CookieInternal.VersionAttributeName, CookieToken.Version),
            new RecognizedAttribute(CookieInternal.DomainAttributeName, CookieToken.Domain),
            new RecognizedAttribute(CookieInternal.SecureAttributeName, CookieToken.Secure),
            new RecognizedAttribute(CookieInternal.DiscardAttributeName, CookieToken.Discard),
            new RecognizedAttribute(CookieInternal.PortAttributeName, CookieToken.Port),
            new RecognizedAttribute(CookieInternal.CommentAttributeName, CookieToken.Comment),
            new RecognizedAttribute(CookieInternal.CommentUrlAttributeName, CookieToken.CommentUrl),
            new RecognizedAttribute(CookieInternal.HttpOnlyAttributeName, CookieToken.HttpOnly),
        };

        static RecognizedAttribute[] RecognizedServerAttributes = {
            new RecognizedAttribute('$' + CookieInternal.PathAttributeName, CookieToken.Path),
            new RecognizedAttribute('$' + CookieInternal.VersionAttributeName, CookieToken.Version),
            new RecognizedAttribute('$' + CookieInternal.DomainAttributeName, CookieToken.Domain),
            new RecognizedAttribute('$' + CookieInternal.PortAttributeName, CookieToken.Port),
            new RecognizedAttribute('$' + CookieInternal.HttpOnlyAttributeName, CookieToken.HttpOnly),
        };

        internal CookieToken TokenFromName(bool parseResponseCookies)
        {
            if (!parseResponseCookies)
            {
                for (int i = 0; i < RecognizedServerAttributes.Length; ++i)
                {
                    if (RecognizedServerAttributes[i].IsEqualTo(Name))
                    {
                        return RecognizedServerAttributes[i].Token;
                    }
                }
            }
            else
            {
                for (int i = 0; i < RecognizedAttributes.Length; ++i)
                {
                    if (RecognizedAttributes[i].IsEqualTo(Name))
                    {
                        return RecognizedAttributes[i].Token;
                    }
                }
            }
            return CookieToken.Unknown;
        }
    }
}
