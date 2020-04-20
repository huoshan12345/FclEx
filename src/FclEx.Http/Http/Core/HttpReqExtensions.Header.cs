using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using MoreLinq;

namespace FclEx.Http.Core
{
    public static partial class HttpReqExtensions
    {
        public static HttpReq AddHeader(this HttpReq req, IEnumerable<KeyValuePair<string, string>> paras)
        {
            paras?.ForEach(m => req.AddHeader(m));
            return req;
        }

        public static HttpReq AddHeader(this HttpReq req, KeyValuePair<string, string> pair) => req.AddHeader(pair.Key, pair.Value);

        public static HttpReq AddHeaderPair(this HttpReq req, string queryPair, char sepetator = ':')
        {
            var pair = queryPair.Split(sepetator);
            req.AddHeader(pair[0], pair.Length > 1 ? pair[1] : "");
            return req;
        }

        public static HttpReq AcceptUtf8(this HttpReq req)
        {
            return req.AddHeader("Accept-Charset", "utf-8");
        }

        public static HttpReq AcceptCn(this HttpReq req)
        {
            return req.AddHeader("Accept-Language", "zh-CN,zh;q=0.8");
        }

        public static HttpReq Ajax(this HttpReq req)
        {
            return req.AddHeader("X-Requested-With", "XMLHttpRequest");
        }

        public static HttpReq Compress(this HttpReq req)
            => req.AddHeader(HttpKnownHeaderNames.AcceptEncoding, "gzip, deflate");

        public static HttpReq Referrer(this HttpReq req, string referrer)
        {
            req.Referrer = referrer;
            return req;
        }

        public static HttpReq TryReferrer(this HttpReq req, string referrer)
        {
            req.Referrer ??= referrer;
            return req;
        }

        public static HttpReq UserAgent(this HttpReq req, string userAgent)
        {
            req.UserAgent = userAgent;
            return req;
        }

        public static HttpReq TryUserAgent(this HttpReq req, string userAgent)
        {
            req.UserAgent ??= userAgent;
            return req;
        }

        public static string GetRequestHeader(this HttpReq req, string? cookieHeader = null)
        {
            var sb = new StringBuilder();
            foreach (var pair in req.HeaderMap)
                sb.AppendLine($"{pair.Key}: { pair.Value}");
            if (!req.HeaderMap.ContainsKey(HttpKnownHeaderNames.Cookie) && !cookieHeader.IsNullOrEmpty())
                sb.Append(HttpKnownHeaderNames.Cookie + ": " + cookieHeader);
            return sb.ToString();
        }

        public static string GetRequestHeader(this HttpReq req, IEnumerable<Cookie> cookies)
        {
            return GetRequestHeader(req, cookies.Select(m => m.ToString()).JoinWith("; "));
        }

    }
}
