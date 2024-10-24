namespace FclEx.Helpers;

public static class StringHelper
{
    /// <summary>
    /// 把十六进制字符串转在byte[]
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public static byte[] HexStringToBytes(string hex)
    {
        if (hex.Length == 0)
        {
            return Array.Empty<byte>();
        }
        if (hex.Length % 2 == 1)
        {
            hex = "0" + hex;
        }

        return Enumerable.Range(0, hex.Length / 2)
            .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
            .ToArray();
    }

    public static string PadLeftWith(string source, int blockSize, char padChar) => PadWith(source, blockSize, padChar, true);

    public static string PadRightWith(string source, int blockSize, char padChar) => PadWith(source, blockSize, padChar, false);

    private static string PadWith(string source, int blockSize, char padChar, bool padLeft)
    {
        var padLength = blockSize - source.Length % blockSize;
        if (padLength == 0) return source;
        var totalWidth = padLength + source.Length;

        return padLeft
            ? source.PadLeft(totalWidth, padChar)
            : source.PadRight(totalWidth, padChar);
    }

}