using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FclEx.Http.Services;
using MoreLinq;
using Newtonsoft.Json;

namespace FclEx.Http.Core
{
    public static partial class HttpReqExtensions
    {
        public static HttpReq AddQueryValue<T>(this HttpReq req, string key, T? value) => req.AddQueryValue(key, value.ToStringOrEmpty());

        public static HttpReq AddQueryValue(this HttpReq req, KeyValuePair<string, string?> pair) => req.AddQueryValue(pair.Key, pair.Value);

        public static HttpReq AddQueryValue(this HttpReq req, Tuple<string, string?> pair) => req.AddQueryValue(pair.Item1, pair.Item2);

        public static HttpReq AddQueryValue(this HttpReq req, (string, string?) pair) => req.AddQueryValue(pair.Item1, pair.Item2);

        public static HttpReq AddQueryValue(this HttpReq req, IEnumerable<KeyValuePair<string, string?>> paras)
        {
            paras.ForEach(m => req.AddQueryValue(m));
            return req;
        }

        public static HttpReq AddQueryPair(this HttpReq req, string queryPair, char sepetator = ':')
        {
            var pair = queryPair.Split(sepetator);
            return req.AddQueryValue(pair[0], pair.Length > 1 ? pair[1] : "");
        }

        public static HttpReq AddFormValue<T>(this HttpReq req, string key, T? value) => req.AddFormValue(key, value.ToStringOrEmpty());

        public static HttpReq AddFormValue(this HttpReq req, KeyValuePair<string, string?> pair) => req.AddFormValue(pair.Key, pair.Value);

        public static HttpReq AddFormValue(this HttpReq req, Tuple<string, string?> pair) => req.AddFormValue(pair.Item1, pair.Item2);

        public static HttpReq AddFormValue(this HttpReq req, (string, string?) pair) => req.AddFormValue(pair.Item1, pair.Item2);

        public static HttpReq AddFormValue(this HttpReq req, IEnumerable<KeyValuePair<string, string?>> paras)
        {
            paras?.ForEach(m => req.AddFormValue(m));
            return req;
        }

        public static HttpReq AddFormPair(this HttpReq req, string queryPair, char sepetator = ':')
        {
            var pair = queryPair.Split(sepetator);
            return req.AddFormValue(pair[0], pair.Length > 1 ? pair[1] : "");
        }

        public static HttpReq AddDataIfNotEmpty(this HttpReq req, string key, string? value)
        {
            return AddDataIf(req, !value.IsNullOrEmpty(), key, value);
        }

        public static HttpReq AddDataIf(this HttpReq req, bool condition, string key, string? value)
        {
            return condition ? AddData(req, key, value) : req;
        }

        public static HttpReq AddData(this HttpReq req, string key, string? value)
        {
            return req.Method == HttpMethodType.Get
                ? req.AddQueryValue(key, value)
                : req.AddFormValue(key, value);
        }

        public static HttpReq AddData<T>(this HttpReq req, string key, T? value)
        {
            return AddData(req, key, value.ToStringOrEmpty());
        }

        public static HttpReq AddData(this HttpReq req, IEnumerable<KeyValuePair<string, string?>> paras)
        {
            return req.Method == HttpMethodType.Get
                ? req.AddQueryValue(paras)
                : req.AddFormValue(paras);
        }

        public static HttpReq AddDataPair(this HttpReq req, string queryPair, char sepetator = ':')
        {
            return req.Method == HttpMethodType.Get
                ? req.AddQueryPair(queryPair, sepetator)
                : req.AddFormPair(queryPair, sepetator);
        }

        public static HttpReq Body(this HttpReq req, string data)
        {
            return req.Body(data.ToBytes(req.Encoding));
        }

        public static HttpReq Body(this HttpReq req, byte[] data)
        {
            req.Body = data.ToSegment();
            return req;
        }

        public static HttpReq Body(this HttpReq req, byte[] data, int offset, int count)
        {
            req.Body = data.ToSegment(offset, count);
            return req;
        }

        public static HttpReq Body(this HttpReq req, ArraySegment<byte> data)
        {
            req.Body = data;
            return req;
        }

        public static HttpReq JsonBody<T>(this HttpReq req, T data, JsonOptions options = default)
        {
            return req.Body(data.ToJson(options));
        }

        public static HttpReq AddFile(this HttpReq req, HttpFileUploadInfo fileUpload, byte[] fileBytes)
        {
            req.FileMap[fileUpload] = fileBytes;
            return req;
        }

        public static HttpReq AddDataIfValid(this HttpReq req, string key, string? value)
        {
            return req.AddDataIf(value.IsValid(), key, value!);
        }
    }
}
