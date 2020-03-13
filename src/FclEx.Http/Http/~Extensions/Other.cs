using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Core;

namespace FclEx.Http
{
    public static class Other
    {
        public static StringBuilder AppendHttpLine(this StringBuilder sb, string value)
        {
            return sb.Append(value + HttpConstants.NewLine);
        }

        public static async Task CopyToAsync(this Stream source, Stream dest, CancellationToken token, TimeSpan? timeout, int bufferSize = 256 * 1024)
        {
            var pool = ArrayPool<byte>.Shared;
            var buffer = pool.Rent(bufferSize);
            try
            {
                int bytesCopied;
                do
                {
                    using var cts = WithTimeout(token, timeout);
                    bytesCopied = await source.ReadAsync(buffer, 0, buffer.Length, cts.Token).DonotCapture();
                    await dest.WriteAsync(buffer, 0, bytesCopied, cts.Token).DonotCapture();
                } while (bytesCopied > 0);
            }
            finally
            {
                pool.Return(buffer);
            }
        }

        public static CancellationTokenSource WithTimeout(this CancellationToken token, TimeSpan? timeout)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            if (timeout.HasValue)
            {
                cts.CancelAfter(timeout.Value);
            }
            return cts;
        }
    }
}
