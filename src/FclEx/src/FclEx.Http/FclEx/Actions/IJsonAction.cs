using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FclEx;
using FclEx.Extensions;
using FclEx.Http;
using FclEx.Utils;
using Newtonsoft.Json.Linq;

namespace FclEx.Actions
{
    public interface IJsonAction<T> : IHttpResHandler<T>
    {
        string? JsonResultPath { get; }

        OperateResult<T> IHttpResHandler<T>.GetResult(HttpRes res)
        {
            var (successful, str, ex, _) = GetJson(res);
            if (!successful)
                return ex!;

            var context = new JsonActionContext(res, str!, JsonResultPath);

            if (IsFailed(context))
                return HandleFailed(context);

            return GetResult(context);
        }

        OperateResult<string> GetJson(HttpRes response)
        {
            var str = response.ResponseString;
            return str.IsPossibleJson()
                ? Operate.CreateSuccess(response.ResponseString)
                : Operate.CreateError<string>("The res string is not a valid json: " + str.TruncateSafely(256));
        }

        bool IsFailed(JsonActionContext context) => !context.ResultTokens.Any();

        OperateResult<T> HandleFailed(JsonActionContext context)
        {
            const string msg = "The result object does not exist in json";
            var error = JsonResultPath == null ? msg : msg + " at " + JsonResultPath;
            error = error + ": " + context.Json.TruncateSafely(256);
            return error;
        }

        OperateResult<T> GetResult(JsonActionContext context)
        {
            return context.ResultToken!.ToObject<T>()!;
        }
    }

    public interface IJsonAction : IJsonAction<Unit>
    {
        OperateResult<Unit> IJsonAction<Unit>.GetResult(JsonActionContext context) => Operate.Success;
    }

    public readonly struct JsonActionContext
    {
        public JsonActionContext(HttpRes httpRes, string json, string? path)
        {
            HttpRes = httpRes;
            Json = json;
            Path = path;
            Token = JToken.Parse(json);
            ResultTokens = path == null
                ? Token.Yield()
                : Token.SelectTokens(path)!;
        }

        public HttpRes HttpRes { get; }
        public string? Path { get; }
        public string Json { get; }
        public JToken Token { get; }
        public IEnumerable<JToken> ResultTokens { get; }
        public JToken? ResultToken => ResultTokens.FirstOrDefault();
    }
}
