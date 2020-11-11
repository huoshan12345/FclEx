using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dawn;

namespace FclEx.Helpers
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    partial class EncodingHelper
    {
        private const int UTF8CodePage = 65001;
        private const int UTF8PreambleLength = 3;
        private const byte UTF8PreambleByte0 = 0xEF;
        private const byte UTF8PreambleByte1 = 0xBB;
        private const byte UTF8PreambleByte2 = 0xBF;
        private const int UTF8PreambleFirst2Bytes = 0xEFBB;

        private const int UTF32CodePage = 12000;
        private const int UTF32PreambleLength = 4;
        private const byte UTF32PreambleByte0 = 0xFF;
        private const byte UTF32PreambleByte1 = 0xFE;
        private const byte UTF32PreambleByte2 = 0x00;
        private const byte UTF32PreambleByte3 = 0x00;
        private const int UTF32OrUnicodePreambleFirst2Bytes = 0xFFFE;

        private const int UnicodeCodePage = 1200;
        private const int UnicodePreambleLength = 2;
        private const byte UnicodePreambleByte0 = 0xFF;
        private const byte UnicodePreambleByte1 = 0xFE;

        private const int BigEndianUnicodeCodePage = 1201;
        private const int BigEndianUnicodePreambleLength = 2;
        private const byte BigEndianUnicodePreambleByte0 = 0xFE;
        private const byte BigEndianUnicodePreambleByte1 = 0xFF;
        private const int BigEndianUnicodePreambleFirst2Bytes = 0xFEFF;

        public static int GetPreambleLength(ArraySegment<byte> buffer, Encoding encoding)
        {
            Guard.Argument(encoding, nameof(encoding)).NotNull();

            var data = buffer.Array ?? throw new ArgumentNullException(nameof(buffer.Array));
            var offset = buffer.Offset;
            var dataLength = buffer.Count;

            switch (encoding.CodePage)
            {
                case UTF8CodePage:
                    return (dataLength >= UTF8PreambleLength
                            && data[offset + 0] == UTF8PreambleByte0
                            && data[offset + 1] == UTF8PreambleByte1
                            && data[offset + 2] == UTF8PreambleByte2) ? UTF8PreambleLength : 0;
                case UTF32CodePage:
                    return (dataLength >= UTF32PreambleLength
                            && data[offset + 0] == UTF32PreambleByte0
                            && data[offset + 1] == UTF32PreambleByte1
                            && data[offset + 2] == UTF32PreambleByte2
                            && data[offset + 3] == UTF32PreambleByte3) ? UTF32PreambleLength : 0;
                case UnicodeCodePage:
                    return (dataLength >= UnicodePreambleLength
                            && data[offset + 0] == UnicodePreambleByte0
                            && data[offset + 1] == UnicodePreambleByte1) ? UnicodePreambleLength : 0;

                case BigEndianUnicodeCodePage:
                    return (dataLength >= BigEndianUnicodePreambleLength
                            && data[offset + 0] == BigEndianUnicodePreambleByte0
                            && data[offset + 1] == BigEndianUnicodePreambleByte1) ? BigEndianUnicodePreambleLength : 0;

                default:
                    var preamble = encoding.GetPreamble();
                    return BufferHasPrefix(buffer, preamble) ? preamble.Length : 0;
            }
        }

        private static bool BufferHasPrefix(ArraySegment<byte> buffer, byte[] prefix)
        {
            var byteArray = buffer.Array;
            if (prefix == null || byteArray == null || prefix.Length > buffer.Count || prefix.Length == 0)
                return false;

            for (int i = 0, j = buffer.Offset; i < prefix.Length; i++, j++)
            {
                if (prefix[i] != byteArray[j])
                    return false;
            }

            return true;
        }

        public static bool TryDetectEncoding(ArraySegment<byte> buffer, out Encoding? encoding, out int preambleLength)
        {
            var data = buffer.Array ?? throw new ArgumentNullException(nameof(buffer.Array));
            var offset = buffer.Offset;
            var dataLength = buffer.Count;

            if (dataLength >= 2)
            {
                var first2Bytes = data[offset + 0] << 8 | data[offset + 1];

                switch (first2Bytes)
                {
                    case UTF8PreambleFirst2Bytes:
                        if (dataLength >= UTF8PreambleLength && data[offset + 2] == UTF8PreambleByte2)
                        {
                            encoding = Encoding.UTF8;
                            preambleLength = UTF8PreambleLength;
                            return true;
                        }
                        break;

                    case UTF32OrUnicodePreambleFirst2Bytes:
                        // UTF32 not supported on Phone
                        if (dataLength >= UTF32PreambleLength && data[offset + 2] == UTF32PreambleByte2 && data[offset + 3] == UTF32PreambleByte3)
                        {
                            encoding = Encoding.UTF32;
                            preambleLength = UTF32PreambleLength;
                        }
                        else
                        {
                            encoding = Encoding.Unicode;
                            preambleLength = UnicodePreambleLength;
                        }
                        return true;

                    case BigEndianUnicodePreambleFirst2Bytes:
                        encoding = Encoding.BigEndianUnicode;
                        preambleLength = BigEndianUnicodePreambleLength;
                        return true;
                }
            }

            encoding = null;
            preambleLength = 0;
            return false;
        }
    }
}
