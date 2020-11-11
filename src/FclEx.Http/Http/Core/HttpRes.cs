using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.Collections.Extensions;

namespace FclEx.Http.Core
{
    public class HttpRes
    {
        public HttpRes(HttpReq req)
        {
            Req = req ?? throw new ArgumentNullException(nameof(req));
        }
        private List<Uri>? _redirectUris;
        private MultiValueDictionary<string, string?>? _headers;

        public string? Location => Headers.GetFirstOr(HttpKnownHeaderNames.Location);
        public bool HasError => Exception != null;
        public HttpReq Req { get; }
        public string ResponseString { get; internal set; } = string.Empty;
        public Encoding? Encoding { get; internal set; }
        public byte[] ResponseBytes { get; internal set; } = Array.Empty<byte>();
        public Exception? Exception { get; internal set; }
        public TimeSpan ExcuteTime { get; internal set; }
        public DateTime RequestUtcTime { get; internal set; }
        public MultiValueDictionary<string, string?> Headers => _headers ??= new MultiValueDictionary<string, string?>(StringComparer.InvariantCultureIgnoreCase);
        public HttpStatusCode StatusCode { get; internal set; }
        public List<Uri> RedirectUris => _redirectUris ??= new List<Uri>();

        internal static readonly HttpRes EmptyRes = new HttpRes(new HttpReq(string.Empty, HttpMethodType.Get));
        public static HttpRes CreateError(HttpReq req, Exception e) => new HttpRes(req) { Exception = e };
    }
}
