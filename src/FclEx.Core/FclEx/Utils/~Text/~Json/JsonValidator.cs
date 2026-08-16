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

    static bool ParseValue(string s, ref int i, int depth)
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
        }

        if (c == '-' || char.IsDigit(c))
            return ParseNumber(s, ref i);

        if (Match(s, ref i, "true"))
            return true;

        if (Match(s, ref i, "false"))
            return true;

        if (Match(s, ref i, "null"))
            return true;

        return false;
    }

    static bool ParseObject(string s, ref int i, int depth)
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

    static bool ParseArray(string s, ref int i, int depth)
    {
        if (depth > MaxDepth)
            return false;

        if (!Consume(s, ref i, '['))
            return false;

        SkipWhitespace(s, ref i);

        if (Consume(s, ref i, ']'))
            return true;

        while (true)
        {
            if (!ParseValue(s, ref i, depth))
                return false;

            SkipWhitespace(s, ref i);

            if (Consume(s, ref i, ']'))
                return true;

            if (!Consume(s, ref i, ','))
                return false;
        }
    }

    static bool ParseString(string s, ref int i)
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
                        if (i >= s.Length || !IsHex(s[i++]))
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

    static bool ParseNumber(string s, ref int i)
    {
        var start = i;

        if (i < s.Length && s[i] == '-') i++;

        if (i >= s.Length)
            return false;

        if (s[i] == '0')
        {
            i++;
        }
        else if (char.IsDigit(s[i]))
        {
            while (i < s.Length && char.IsDigit(s[i])) i++;
        }
        else return false;

        if (i < s.Length && s[i] == '.')
        {
            i++;

            if (i >= s.Length || !char.IsDigit(s[i]))
                return false;

            while (i < s.Length && char.IsDigit(s[i]))
                i++;
        }

        if (i < s.Length && (s[i] == 'e' || s[i] == 'E'))
        {
            i++;
            if (i < s.Length && (s[i] == '+' || s[i] == '-'))
                i++;

            if (i >= s.Length || !char.IsDigit(s[i]))
                return false;

            while (i < s.Length && char.IsDigit(s[i]))
                i++;
        }

        return i > start;
    }

    static void SkipWhitespace(string s, ref int i)
    {
        while (i < s.Length && char.IsWhiteSpace(s[i]))
            i++;
    }

    static bool Consume(string s, ref int i, char c)
    {
        if (i < s.Length && s[i] == c)
        {
            i++;
            return true;
        }
        return false;
    }

    static bool Match(string s, ref int i, string keyword)
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

    static bool IsHex(char c)
    {
        return c is >= '0' and <= '9'
            or >= 'a' and <= 'f'
            or >= 'A' and <= 'F';
    }
}
