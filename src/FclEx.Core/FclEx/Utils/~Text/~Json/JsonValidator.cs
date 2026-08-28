namespace FclEx.Utils;

public static class JsonValidator
{
    /// <summary>
    /// The maximum permitted number of nested JSON objects and arrays.
    /// </summary>
    public const int MaxDepth = 64;

    public static bool IsValid(string s)
    {
        var i = 0;
        SkipWhitespace(s, ref i);

        var ok = ParseValue(s, ref i, 0);

        SkipWhitespace(s, ref i);
        return ok && i == s.Length;
    }

    private static bool ParseValue(string s, ref int i, int depth)
    {
        SkipWhitespace(s, ref i);

        if (i >= s.Length)
            return false;

        var c = s[i];

        switch (c)
        {
            case '{': return ParseObject(s, ref i, depth + 1);
            case '[': return ParseArray(s, ref i, depth + 1);
            case '"': return ParseString(s, ref i);
            case 't': return Match(s, ref i, "true");
            case 'f': return Match(s, ref i, "false");
            case 'n': return Match(s, ref i, "null");
        }

        if (c == '-' || c.IsAsciiDigit())
            return ParseNumber(s, ref i);

        return false;
    }

    private static bool ParseObject(string s, ref int i, int depth)
    {
        if (depth > MaxDepth)
            return false;

        if (!Consume(s, ref i, '{'))
            return false;

        SkipWhitespace(s, ref i);

        if (Consume(s, ref i, '}'))
            return true;

        while (true)
        {
            SkipWhitespace(s, ref i);

            if (!ParseString(s, ref i))
                return false;

            SkipWhitespace(s, ref i);

            if (!Consume(s, ref i, ':'))
                return false;

            if (!ParseValue(s, ref i, depth))
                return false;

            SkipWhitespace(s, ref i);

            if (Consume(s, ref i, '}'))
                return true;

            if (!Consume(s, ref i, ','))
                return false;
        }
    }

    private static bool ParseArray(string s, ref int i, int depth)
    {
        if (depth > MaxDepth)
            return false;

        if (Consume(s, ref i, '[') == false)
            return false;

        SkipWhitespace(s, ref i);

        if (Consume(s, ref i, ']'))
            return true;

        while (true)
        {
            if (ParseValue(s, ref i, depth) == false)
                return false;

            SkipWhitespace(s, ref i);

            if (Consume(s, ref i, ']'))
                return true;

            if (Consume(s, ref i, ',') == false)
                return false;
        }
    }

    private static bool ParseString(string s, ref int i)
    {
        if (!Consume(s, ref i, '"'))
            return false;

        while (i < s.Length)
        {
            var c = s[i++];

            if (c == '"')
                return true;

            if (c == '\\')
            {
                if (i >= s.Length)
                    return false;

                var esc = s[i++];

                // Check for valid escape sequences
                if ("\"\\/bfnrt".Contains(esc))
                    continue;

                if (esc == 'u')
                {
                    for (var k = 0; k < 4; k++)
                    {
                        if (i >= s.Length || s[i++].IsHex() == false)
                            return false;
                    }
                    continue;
                }

                return false;
            }

            if (c < 0x20)
                return false;
        }

        return false;
    }

    private static bool ParseNumber(string s, ref int i)
    {
        var start = i;

        if (i < s.Length && s[i] == '-')
            i++;

        if (i >= s.Length)
            return false;

        if (s[i] == '0')
        {
            i++;
        }
        else if (s[i].IsAsciiDigit())
        {
            while (i < s.Length && s[i].IsAsciiDigit())
                i++;
        }
        else return false;

        if (i < s.Length && s[i] == '.')
        {
            i++;

            if (i >= s.Length || !s[i].IsAsciiDigit())
                return false;

            while (i < s.Length && s[i].IsAsciiDigit())
                i++;
        }

        if (i < s.Length && (s[i] == 'e' || s[i] == 'E'))
        {
            i++;

            if (i < s.Length && (s[i] == '+' || s[i] == '-'))
                i++;

            if (i >= s.Length || !s[i].IsAsciiDigit())
                return false;

            while (i < s.Length && s[i].IsAsciiDigit())
                i++;
        }

        return i > start;
    }

    private static void SkipWhitespace(string s, ref int i)
    {
        while (i < s.Length && s[i].IsJsonWhitespace())
            i++;
    }

    private static bool Consume(string s, ref int i, char c)
    {
        if (i < s.Length && s[i] == c)
        {
            i++;
            return true;
        }
        return false;
    }

    private static bool Match(string s, ref int i, string keyword)
    {
        var len = keyword.Length;

        if (i + len > s.Length)
            return false;

        for (var k = 0; k < len; k++)
        {
            if (s[i + k] != keyword[k])
                return false;
        }

        i += len;
        return true;
    }
}

file static class Extensions
{
    /// <summary>
    /// Determines whether the specified character is a whitespace character according to JSON specification. <br/>
    /// Does not include \v (Vertical Tab), \f (Form Feed), U+00A0 (NBSP), U+2003 (EM SPACE)
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is a JSON whitespace character; otherwise, <see langword="false"/>.</returns>
    public static bool IsJsonWhitespace(this char c)
    {
        return c is ' ' or '\t' or '\n' or '\r';
    }
}
