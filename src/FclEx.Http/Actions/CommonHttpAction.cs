using System;
using System.Collections.Concurrent;
using System.Reflection;
using FclEx.Http.Core;
using FclEx.Http.Services;

namespace FclEx.Actions
{
    public abstract class CommonHttpAction : AbstractHttpAction
    {
        protected abstract string Url { get; }

        protected abstract HttpReqType ReqType { get; }

        protected override HttpReq BuildRequest()
        {
            var req = HttpReq.Create(Url, ReqType)
                .Compress();
            ModifyRequest(req);
            return req;
        }

        protected virtual void ModifyRequest(HttpReq req) { }

        protected string GetUrl(ConcurrentDictionary<Type, string> apiDic, Type apiType)
        {
            var actionType = GetType();
            return apiDic.GetOrAdd(actionType, key =>
            {
                var urlName = key.Name.Replace("Action", "");
                var value = apiType.GetTypeInfo().GetField(urlName)?.GetValue(null);
                if (value == null) throw new Exception(key.Name + "获取Url失败");
                return value.ToString();
            });
        }

        protected CommonHttpAction(IHttpService httpService) : base(httpService)
        {
        }
    }
}
