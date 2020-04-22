using System;
using System.Collections.Generic;
using System.Text;
using Dawn;
using FclEx.Extensions.Json;
using FclEx.Http.Core;
using FclEx.Http.Services;
using FclEx.Utils;
using Newtonsoft.Json.Linq;

namespace FclEx.Actions
{
    public static class Extensions
    {
        public static IAction<HttpRes> ToAction(this HttpReq req, IHttpService? httpService = null, bool unwrapError = true)
        {
            return (new HttpReqAction(req, httpService ?? HttpClientService.Default, unwrapError));
        }

        public static IAction<T> ReadJson<T>(this IAction<HttpRes> action, string? path = null)
        {
            return action.Bind(m => m.ReadJson<T>(path));
        }

        public static IAction<HttpRes> NextReq<T>(this IAction<(HttpRes, T)> action, Func<T, HttpReq> func, 
            IHttpService? httpService = null, bool unwrapError = true)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return action.Next((res, data) => func(data).ToAction(httpService, unwrapError));
        }

        public static IAction<HttpRes> NextReq<T>(this IAction<T> action, Func<T, HttpReq> func,
            IHttpService? httpService = null, bool unwrapError = true)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return action.Next(data => func(data).ToAction(httpService, unwrapError));
        }
    }
}
