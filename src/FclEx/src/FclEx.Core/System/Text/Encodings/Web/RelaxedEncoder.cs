namespace System.Text.Encodings.Web;

/// <summary>
/// JavaScript encoder built on top of <see cref="JavaScriptEncoder.UnsafeRelaxedJsonEscaping"/> that does not escape emoji.
/// </summary>
public class RelaxedEncoder : JavaScriptEncoder
{
    public static readonly JavaScriptEncoder Instance = new RelaxedEncoder();

    private readonly JavaScriptEncoder _defaultEncoder = UnsafeRelaxedJsonEscaping;

    private RelaxedEncoder() { }

    public override int MaxOutputCharactersPerInputCharacter => _defaultEncoder.MaxOutputCharactersPerInputCharacter;

    public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
    {
        var input = new ReadOnlySpan<char>(text, textLength);
        var idx = 0;

        // Enumerate until we're out of data or saw invalid input
        while (Rune.DecodeFromUtf16(input[idx..], out var result, out var charsConsumed) == OperationStatus.Done)
        {
            if (WillEncode(result.Value))
            {
                // found a char that needs to be escaped
                break;
            }

            idx += charsConsumed;
        }

        if (idx == input.Length)
        {
            // walked entire input without finding a char which needs escaping
            return -1;
        }

        return idx;
    }

    public override unsafe bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
    {
        return _defaultEncoder.TryEncodeUnicodeScalar(unicodeScalar, buffer, bufferLength, out numberOfCharactersWritten);
    }

    public override bool WillEncode(int unicodeScalar)
    {
        return UnicodeScalarHelper.IsEmoji(unicodeScalar) == false && _defaultEncoder.WillEncode(unicodeScalar);
    }
}