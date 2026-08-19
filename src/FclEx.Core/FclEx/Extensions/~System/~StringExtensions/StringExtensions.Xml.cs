namespace FclEx.Extensions;

partial class StringExtensions
{
    /// <summary>
    /// Performs a cheap, conservative precheck for whether <paramref name="text"/> could be an XML document.
    /// </summary>
    /// <remarks>
    /// A <see langword="false"/> result means the text cannot be a well-formed XML document. A
    /// <see langword="true"/> result does not establish well-formedness; use an XML parser when validation is required.
    /// This method only checks the document envelope and does not inspect elements, attributes, entities, or nesting.
    /// </remarks>
    public static bool CouldBeXmlDocument([NotNullWhen(true)] this string? text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        var value = text!;
        var start = 0;
        var end = value.Length - 1;

        while (start <= end && char.IsWhiteSpace(value[start]))
            start++;

        if (start <= end && value[start] == '\uFEFF')
            start++;

        while (start <= end && char.IsWhiteSpace(value[start]))
            start++;

        while (end >= start && char.IsWhiteSpace(value[end]))
            end--;

        return start <= end && value[start] == '<' && value[end] == '>';
    }
}
