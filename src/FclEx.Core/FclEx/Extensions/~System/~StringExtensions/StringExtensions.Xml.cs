using System.Xml;

namespace FclEx.Extensions;

partial class StringExtensions
{
    /// <summary>
    /// Performs a lightweight XML 1.0 precheck of characters, the document envelope, and the root start tag.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="false"/> for null or empty input, illegal XML characters, or a missing root start tag.
    /// Leading declarations, processing instructions, comments, and a DOCTYPE are skipped without loading external resources.
    /// DOCTYPE scanning respects quoted values and internal subsets; it does not validate DTD syntax or expand entities.
    /// A <see langword="true"/> result does not establish well-formedness: attributes, entity references, matching end tags,
    /// and element nesting are not validated. Use an XML parser when full validation is required.
    /// Whitespace and an initial Unicode BOM are tolerated. HTML is not rejected solely because it has an HTML DOCTYPE.
    /// The method scans the entire input for illegal characters and allocates a substring for the root element name.
    /// </remarks>
    public static bool CouldBeXmlDocument([NotNullWhen(true)] this string? text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        var value = text!;
        for (var i = 0; i < value.Length; i++)
        {
            if (XmlConvert.IsXmlChar(value[i]))
                continue;

            if (i + 1 < value.Length && char.IsSurrogatePair(value[i], value[i + 1]))
            {
                i++;
                continue;
            }

            return false;
        }

        var start = 0;
        var end = value.Length - 1;
        SkipWhitespace();
        if (start <= end && value[start] == '\uFEFF')
            start++;
        SkipWhitespace();
        while (end >= start && char.IsWhiteSpace(value[end]))
            end--;

        if (start > end || value[end] != '>')
            return false;

        var hasDoctype = false;
        while (start < end)
        {
            if (StartsWith("<!--"))
            {
                if (!SkipDelimited("-->", 4))
                    return false;
            }
            else if (StartsWith("<?"))
            {
                if (!SkipDelimited("?>", 2))
                    return false;
            }
            else if (StartsWith("<!DOCTYPE") && start + 9 <= end && char.IsWhiteSpace(value[start + 9]))
            {
                if (hasDoctype || !SkipDoctype())
                    return false;
                hasDoctype = true;
            }
            else
            {
                return HasRootStartTag();
            }

            SkipWhitespace();
        }

        return false;

        void SkipWhitespace()
        {
            while (start <= end && char.IsWhiteSpace(value[start]))
                start++;
        }

        bool StartsWith(string token)
        {
            return start + token.Length <= end + 1
                   && string.CompareOrdinal(value, start, token, 0, token.Length) == 0;
        }

        bool SkipDelimited(string terminator, int prefixLength)
        {
            var position = value.IndexOf(terminator, start + prefixLength, StringComparison.Ordinal);
            if (position < 0)
                return false;

            start = position + terminator.Length;
            return true;
        }

        bool SkipDoctype()
        {
            start += 9;
            var subsetDepth = 0;
            var quote = '\0';
            while (start <= end)
            {
                var c = value[start];
                if (quote != '\0')
                {
                    if (c == quote)
                        quote = '\0';
                }
                else if (StartsWith("<!--"))
                {
                    if (!SkipDelimited("-->", 4))
                        return false;
                    continue;
                }
                else if (StartsWith("<?"))
                {
                    if (!SkipDelimited("?>", 2))
                        return false;
                    continue;
                }
                else if (c is '\'' or '"')
                    quote = c;
                else if (c == '[')
                    subsetDepth++;
                else if (c == ']')
                {
                    if (subsetDepth == 0)
                        return false;
                    subsetDepth--;
                }
                else if (c == '>' && subsetDepth == 0)
                {
                    start++;
                    return true;
                }

                start++;
            }

            return false;
        }

        bool HasRootStartTag()
        {
            if (value[start] != '<')
                return false;

            var nameStart = ++start;
            while (start <= end && !char.IsWhiteSpace(value[start]) && value[start] is not '/' and not '>')
                start++;

            if (start == nameStart)
                return false;

            try
            {
                XmlConvert.VerifyName(value.Substring(nameStart, start - nameStart));
            }
            catch (XmlException)
            {
                return false;
            }

            var quote = '\0';
            while (start <= end)
            {
                var c = value[start++];
                if (c == '<')
                    return false;
                if (quote != '\0')
                {
                    if (c == quote)
                        quote = '\0';
                }
                else if (c is '\'' or '"')
                    quote = c;
                else if (c == '>')
                    return true;
            }

            return false;
        }
    }
}
