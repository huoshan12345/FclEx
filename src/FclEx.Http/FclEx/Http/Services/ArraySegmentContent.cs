using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dawn;

namespace FclEx.Http.Services
{
    internal class ArraySegmentContent : HttpContent
    {
        private readonly ArraySegment<byte> _content;
        private readonly CancellationToken _token;
        private readonly TimeSpan? _timeout;

        public ArraySegmentContent(ArraySegment<byte> content, CancellationToken token, TimeSpan? timeout)
        {
            Guard.Argument(content.Array!, nameof(content.Array)).NotNull();
            _content = content;
            _token = token;
            _timeout = timeout;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            await using var ms = CreateContentReadStream();
            await ms.CopyToAsync(stream, _token, _timeout);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _content.Count;
            return true;
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return CreateContentReadStream().ToTask();
        }

        private Stream CreateContentReadStream()
        {
            return new MemoryStream(_content.Array!, _content.Offset, _content.Count, false);
        }
    }
}
