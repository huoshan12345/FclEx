using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Extensions.Json;
using FclEx.Http;
using FclEx.Http.Core;
using FclEx.Utils;
using FclEx.Web.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FclEx.Actions
{
    public interface IJsonAction<TObject, TResult>
    {
        string? JsonResultPath { get; }

        sealed OperateResult<TResult> GetResult(HttpRes response)
        {
            var (successful, _, str, ex) = GetJsonString(response);
            if (!successful)
                return ex!;

            if (!str.IsPossibleJson())
                return HandleNonJsonResult(response);

            var jToken = str.ToJToken();
            if (!IsSuccessfulJToken(response, jToken))
                return HandleUnsuccessfulJToken(response, str, jToken);

            var resultToken = JsonResultPath == null
                ? jToken
                : jToken.SelectToken(JsonResultPath);

            if (resultToken == null)
                return HandleJsonPathNonExist(response);

            var jsonObjResult = HandleJToken(response, resultToken);
            if (jsonObjResult.HasError())
                return jsonObjResult.ToExplicit<TResult>();

            var obj = jsonObjResult.Result!;
            if (!IsSuccessfulJsonObject(response, obj))
                return HandleUnsuccessfulJsonObject(response, resultToken, obj);

            return HandleResult(response, resultToken, obj);
        }

        OperateResult<TResult> HandleJsonPathNonExist(HttpRes response)
        {
            const string msg = "The result object does not exist in json";
            var error = JsonResultPath == null ? msg : msg + " at " + JsonResultPath;
            return (error + ": " + response.ResponseString.TruncateSafely(256), response.ExcuteTime);
        }

        OperateResult<string> GetJsonString(HttpRes response)
        {
            return OperateResult.CreateSuccess(response.ResponseString);
        }

        OperateResult<TResult> HandleResult(HttpRes response, JToken token, TObject obj);

        bool IsSuccessfulJsonObject(HttpRes response, TObject obj) => true;

        bool IsSuccessfulJToken(HttpRes response, JToken token) => true;

        OperateResult<TObject> HandleJToken(HttpRes response, JToken token)
        {
            return token.ToObject<TObject>()!;
        }

        OperateResult<TResult> HandleUnsuccessfulJToken(HttpRes response, string json, JToken token)
        {
            return "The json is unsuccessful: " + json.TruncateSafely(256);
        }

        OperateResult<TResult> HandleUnsuccessfulJsonObject(HttpRes response, JToken token, TObject obj)
        {
            return "The result is unsuccessful: " + token.ToString().TruncateSafely(256);
        }

        OperateResult<TResult> HandleNonJsonResult(HttpRes response)
        {
            return "The response string is not a valid json: " + response.ResponseString.TruncateSafely(256);
        }
    }

    public interface IUserClientJsonAction<TResult> : IJsonAction<TResult, TResult>
    {
        OperateResult<TResult> IJsonAction<TResult, TResult>.HandleResult(HttpRes response, JToken token, TResult obj)
        {
            return (obj, response.ExcuteTime);
        }

        OperateResult<TResult> IJsonAction<TResult, TResult>.HandleUnsuccessfulJsonObject(HttpRes response, JToken token, TResult obj)
        {
            return HandleUnsuccessfulResult(response, token, obj);
        }

        OperateResult<TResult> HandleUnsuccessfulResult(HttpRes response, JToken token, TResult obj)
        {
            return HandleUnsuccessfulJsonObject(response, token, obj);
        }

        bool IJsonAction<TResult, TResult>.IsSuccessfulJsonObject(HttpRes response, TResult obj) => IsSuccessfulResult(response, obj);

        bool IsSuccessfulResult(HttpRes response, TResult obj) => true;
    }

    public interface IUserClientJsonAction : IUserClientJsonAction<Unit>
    {
        OperateResult<Unit> IJsonAction<Unit, Unit>.HandleJToken(HttpRes response, JToken token) => OperateResult.Success;
        OperateResult<Unit> IJsonAction<Unit, Unit>.HandleResult(HttpRes response, JToken token, Unit obj) => obj;
    }
}
