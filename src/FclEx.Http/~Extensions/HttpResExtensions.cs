using System;
using System.Net;
using System.Threading.Tasks;
using FclEx.Http.Core;
using Newtonsoft.Json.Linq;

namespace FclEx.Http
{
    public static class HttpResExtensions
    {
        public static HttpRes EnsureSuccessStatusCode(this HttpRes res)
        {
            if (res.StatusCode != HttpStatusCode.Created
                && res.StatusCode != HttpStatusCode.OK)
            {
                throw new WebException($"call {res.Req.GetUrl()} with {res.Req.Method} return unsuccessful code: {res.StatusCode}/{res.StatusCode.ToInt()}");
            }
            return res;
        }

        public static HttpRes ThrowIfError(this HttpRes res)
        {
            if (res.HasError) res.Exception.ReThrow();
            return res;
        }

        public static async Task<HttpRes> ThrowIfError(this Task<HttpRes> task)
        {
            var res = await task.DonotCapture();
            res.ThrowIfError();
            return res;
        }

        public static async Task<T> ReadJsonAs<T>(this Task<HttpRes> task)
        {
            var res = await task.DonotCapture();
            res.ThrowIfError();
            if (res.Req.ResultType == HttpResultType.Byte)
                throw new InvalidOperationException("Can not deserialize json from byte array.");
            var resObj = res.ResponseString.ToJToken().ToObject<T>();
            return resObj;
        }
    }
}
