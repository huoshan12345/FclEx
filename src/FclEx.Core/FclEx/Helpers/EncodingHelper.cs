#pragma warning disable SYSLIB0001

namespace FclEx.Helpers;

public static partial class EncodingHelper
{
    public static Encoding Utf8WithoutBom { get; } = new UTF8Encoding(false);

    public static Encoding DetectEncoding(string filePath, Encoding? defaultEncoding = null)
    {
        defaultEncoding ??= Encoding.UTF8;

        using var reader = new StreamReader(filePath, defaultEncoding, true);
        reader.Peek();
        return reader.CurrentEncoding;
    }

    public static Encoding GetEncoding(string filePath, Encoding? defaultEncoding = null)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return GetEncoding(fs, defaultEncoding);
    }

    public static Encoding GetEncoding(Stream stream, Encoding? defaultEncoding = null)
    {
        defaultEncoding ??= Encoding.UTF8;

        var bom = new byte[3];
        var length = stream.Read(bom, 0, 3);
        if (length > 2)
        {
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF) return Encoding.UTF8;
            if (bom[0] == 0xFE && bom[1] == 0xFF && bom[2] == 0x00) return Encoding.BigEndianUnicode;// 也就是大端的UTF-16
            if (bom[0] == 0xFF && bom[1] == 0xFE && bom[2] == 0x41) return Encoding.Unicode;// 也就是小端的UTF-16
        }
        stream.Seek(0, SeekOrigin.Begin);
        return IsUtf8(stream)
            ? Utf8WithoutBom
            : defaultEncoding;
    }

    // 0XXXXXXX
    // 110XXXXX, 10XXXXXX  
    // 1110XXXX, 10XXXXXX, 10XXXXXX  
    // 11110XXX, 10XXXXXX, 10XXXXXX, 10XXXXXX  
    private static bool IsUtf8(Stream stream)
    {
        Check.NotNull(stream);

        using var reader = new BinaryReader(stream);
        var utf8Flag = 0;
        var asciiFlag = 0;
        for (; stream.Position < stream.Length;)
        {
            var curByte = reader.ReadByte();
            if ((curByte & 0x80) == 0)
            {
                asciiFlag++; // 0XXXXXXX
            }
            else if ((curByte & 0xE0) == 0xC0 && stream.Position < stream.Length - 1) // 110xxxxx 10xxxxxx  
            {
                var buff = reader.ReadByte();
                if ((buff & 0x80) != 0x80)
                    return false;

                utf8Flag++;
            }
            else if ((curByte & 0xF0) == 0xE0 && stream.Position < stream.Length - 2) // 1110xxxx 10xxxxxx 10xxxxxx  
            {
                var buff = reader.ReadBytes(2);
                if ((buff[0] & 0x80) != 0x80 || (buff[1] & 0x80) != 0x80)
                    return false;

                utf8Flag++;
            }
            else if ((curByte & 0xF8) == 0xF0 && stream.Position < stream.Length - 3) // 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx  
            {
                var buff = reader.ReadBytes(3);
                if ((buff[0] & 0x80) != 0x80 || (buff[1] & 0x80) != 0x80 || (buff[2] & 0x80) != 0x80)
                    return false;

                utf8Flag++;
            }
            else
            {
                return false;
            }
        }

        return asciiFlag == stream.Length || utf8Flag > 0;
    }
}