using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Dawn;
using FclEx.Extensions;
using FclEx.Helpers;
using FclEx.Utils;

namespace FclEx.Http.Core
{
    public class HttpReq
    {
        private readonly UriCreator _uriCreator;
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public bool ThrowOnNonSuccessCode { get; set; } = true;
        public ArraySegment<byte> Body { get; set; }
        public HttpMethodType Method { get; set; }
        public TimeSpan? TotalTimeout { get; set; } = TimeSpan.FromMinutes(2);
        public TimeSpan? Timeout { get; set; } = TimeSpan.FromSeconds(20);
        public string? CharSet { get; set; }
        public bool DetectCharSetFromHtmlMeta { get; set; }
        public string? FallbackCharSet { get; set; }
        public HttpResultType ResultType { get; set; }
        public bool ReadResultCookie { get; set; } = true;
        public bool ReadResultHeader { get; set; } = true;
        public bool ReadResultContent { get; set; } = true;

        public NameValueCollection QueryMap => _uriCreator.QueryMap;
        public Dictionary<string, string?> HeaderMap { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> FormMap { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<HttpFileUploadInfo, byte[]> FileMap{ get; } = new();

        public string? ContentType
        {
            get
            {
                var type = HeaderMap.Get(HttpKnownHeaderNames.ContentType);
                return type == HttpConstants.MultiPartContentType
                    ? "multipart/form-data; boundary=" + Boundary
                    : type;
            }
            set => HeaderMap[HttpKnownHeaderNames.ContentType] = value;
        }

        public string? Referrer
        {
            get => HeaderMap.Get(HttpKnownHeaderNames.Referrer);
            set => HeaderMap[HttpKnownHeaderNames.Referrer] = value;
        }

        public string? Origin
        {
            get => HeaderMap.Get(HttpKnownHeaderNames.Origin);
            set => HeaderMap[HttpKnownHeaderNames.Origin] = value;
        }

        public string? UserAgent
        {
            get => HeaderMap.Get(HttpKnownHeaderNames.UserAgent);
            set => HeaderMap[HttpKnownHeaderNames.UserAgent] = value;
        }

        public string? Boundary
        {
            get => HeaderMap.Get(HttpConstants.Boundary);
            set => HeaderMap[HttpConstants.Boundary] = value;
        }

        public HttpReq(Uri rawUrl, HttpMethodType method)
        {
            _uriCreator = new UriCreator(rawUrl);

            ContentType = method == HttpMethodType.Get
                ? HttpConstants.DefaultGetContentType
                : HttpConstants.DefaultPostContentType;

            Method = method;

            AddHeader(HttpKnownHeaderNames.UserAgent, HttpConstants.DefaultUserAgent);

            if (UserName.IsValid() && Password.IsValid())
            {
                this.BasicAuth(UserName, Password);
            }
        }

        public HttpReq(HttpMethodType method, string rawUrl)
            : this(rawUrl, method) { }

        public HttpReq(string rawUrl, HttpMethodType method)
            : this(new Uri(rawUrl, UriKind.RelativeOrAbsolute), method) { }

        public static HttpReq Json(Uri url) => new(url, HttpMethodType.Post) { ContentType = HttpConstants.JsonContentType };
        public static HttpReq Json(string url) => Json(new Uri(url, UriKind.RelativeOrAbsolute));

        public static HttpReq Form(Uri url) => new(url, HttpMethodType.Post) { ContentType = HttpConstants.FormContentType };
        public static HttpReq Form(string url) => Form(new Uri(url, UriKind.RelativeOrAbsolute));

        public static HttpReq Get(Uri url) => new(url, HttpMethodType.Get) { ContentType = HttpConstants.DefaultGetContentType };
        public static HttpReq Get(string url) => Get(new Uri(url, UriKind.RelativeOrAbsolute));

        public static HttpReq Upload(Uri url) => new(url, HttpMethodType.Post) { ContentType = HttpConstants.ByteArrayContentType };
        public static HttpReq Upload(string url) => Upload(new Uri(url, UriKind.RelativeOrAbsolute));

        public static HttpReq MultiPart(Uri url) => new(url, HttpMethodType.Post)
        {
            Boundary = "----WebKitFormBoundaryImw0tVH7wlMdFALP",
            ContentType = HttpConstants.MultiPartContentType,
        };
        public static HttpReq MultiPart(string url) => MultiPart(new Uri(url, UriKind.RelativeOrAbsolute));

        public static HttpReq Create(Uri url, HttpReqType reqType)
        {
            switch (reqType)
            {
                case HttpReqType.Get: return Get(url);
                case HttpReqType.Form: return Form(url);
                case HttpReqType.Json: return Json(url);
                case HttpReqType.Upload: return Upload(url);
                case HttpReqType.MultiPart: return MultiPart(url);
                default: throw new ArgumentOutOfRangeException(nameof(reqType), reqType, null);
            }
        }
        public static HttpReq Create(string url, HttpReqType reqType) => Create(new Uri(url, UriKind.RelativeOrAbsolute), reqType);

        public string Fragment
        {
            get => _uriCreator.Fragment;
            set => _uriCreator.Fragment = value;
        }
        public string Host
        {
            get => _uriCreator.Host;
            set => _uriCreator.Host = value;
        }
        public string UserName
        {
            get => _uriCreator.UserName;
            set => _uriCreator.UserName = value;
        }
        public string Password
        {
            get => _uriCreator.Password;
            set => _uriCreator.Password = value;
        }
        public string Path
        {
            get => _uriCreator.Path;
            set => _uriCreator.Path = value;
        }
        public int Port
        {
            get => _uriCreator.Port;
            set => _uriCreator.Port = value;
        }
        public string Scheme
        {
            get => _uriCreator.Scheme;
            set => _uriCreator.Scheme = value;
        }

        public Uri GetUri() => _uriCreator.GetUri();

        public HttpReq AddQueryValue(string key, string? value)
        {
            Guard.Argument(key, nameof(key)).NotNull();
            QueryMap[key.Trim()] = value.ToStringOrEmpty().Trim();
            return this;
        }

        public HttpReq AddFormValue(string key, string? value)
        {
            Guard.Argument(key, nameof(key)).NotNull();
            FormMap[key.Trim()] = value.ToStringOrEmpty().Trim();
            return this;
        }

        public HttpReq AddHeader(string key, string? value)
        {
            Guard.Argument(key, nameof(key)).NotNull();
            HeaderMap[key.Trim()] = value.ToStringOrEmpty().Trim();
            return this;
        }

        public ArraySegment<byte> GetData()
        {
            if (!Body.Array.IsNullOrEmpty())
                return Body;

            var type = HeaderMap.Get(HttpKnownHeaderNames.ContentType);
            switch (type)
            {
                case HttpConstants.FormContentType: return FormMap.ToQueryStr().ToBytes(Encoding).ToSegment();
                case HttpConstants.MultiPartContentType:
                {
                    using var mem = new MemoryStream();
                    using var stringBuilder = ObjectPoolHelper.StringBuilderPool.GetAsDisposable();
                    var sb = stringBuilder.Value;
                    // Write the values
                    foreach (var (key, value) in FormMap)
                    {
                        sb.AppendHttpLine(HttpConstants.EncapsulationBoundary + Boundary);
                        sb.AppendFormat("Content-Disposition: form-data; name=\"{0}\"{1}{1}", key, HttpConstants.NewLine);
                        sb.AppendHttpLine(value);
                    }
                    sb.ToString().ToUtf8Bytes().WriteTo(mem);

                    // Write the files
                    foreach (var (key, value) in FileMap)
                    {
                        var data = new StringBuilder(192);
                        data.AppendHttpLine(HttpConstants.EncapsulationBoundary + Boundary);
                        data.AppendFormat("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", key.Name, key.FileName, HttpConstants.NewLine);
                        data.AppendFormat("Content-Type: {0}{1}{1}", key.ContentType, HttpConstants.NewLine);
                        data.ToString().ToUtf8Bytes().WriteTo(mem);
                        value.WriteTo(mem);
                        HttpConstants.NewLineBytes.WriteTo(mem);
                    }
                    (HttpConstants.EncapsulationBoundary + Boundary + HttpConstants.EncapsulationBoundary).ToUtf8Bytes().WriteTo(mem);
                    return mem.ToArray().ToSegment();
                }
                case HttpConstants.JsonContentType:
                case HttpConstants.ByteArrayContentType:
                default:
                    return Body.Array == null
                        ? Array.Empty<byte>().ToSegment()
                        : Body;
            }
        }
    }
}

