using System.Threading;
using System.Threading.Tasks;
using FclEx.Extensions.Json;
using FclEx.Http;
using FclEx.Http.Core;
using FclEx.Utils;
using FclEx.Web.Core;
using FclEx.Web.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FclEx.Actions
{
    public abstract class UserClientJsonAction<TClient, TJsonObject, TResult> : UserClientAction<TClient, TResult>
        where TClient : IUserClient
    {
        protected UserClientJsonAction(TClient client) : base(client)
        {
            //if (client.Logger.IsEnabled(LogLevel.Trace))
            //    Logger = client.Logger;
        }

        protected override void PreCheckResponse(HttpRes response) { }

        protected override Task<OperateResult<TResult>> HandleResponseAsync(HttpRes response)
        {
            var time = ValueStopwatch.StartNew();
            var result = HandleResponse(response);
            return result.WithElapsed(time.GetElapsedTime() + response.ExcuteTime);
        }

        protected virtual OperateResult<TResult> HandleResponse(HttpRes response)
        {
            if (IsOffline(response))
                return HandleOffline(response);

            if (!IsSuccessfulResponse(response))
                return HandleUnsuccessfulResponse(response);

            if (response.ResponseString.IsNullOrEmpty())
                return HandleEmptyResponse(response);

            var jsonStrResult = GetJsonString(response);
            if (jsonStrResult.HasError())
                return jsonStrResult.ToExplicit<TResult>();

            if (!jsonStrResult.Result.IsPossibleJson())
                return HandleNonJsonResult(response);

            var jToken = jsonStrResult.Result.ToJToken();
            if (!IsSuccessfulJToken(response, jToken))
                return HandleUnsuccessfulJToken(response, jToken);

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

        protected virtual bool IsOffline(HttpRes response) => false;

        protected virtual OperateResult<TResult> HandleOffline(HttpRes response)
        {
            Session.Offline();
            return OperateResult.CreateError<TResult>("The client is offline now for unknown reason", response.ExcuteTime);
        }

        protected virtual OperateResult<TResult> HandleJsonPathNonExist(HttpRes response)
        {
            const string msg = "The result object does not exist in json";
            var error = JsonResultPath == null ? msg : msg + " at " + JsonResultPath;
            return OperateResult.CreateError<TResult>(error + ": " + response.ResponseString.TruncateSafely(256), response.ExcuteTime);
        }

        protected virtual OperateResult<string> GetJsonString(HttpRes response)
        {
            return OperateResult.CreateSuccess(response.ResponseString);
        }

        protected abstract OperateResult<TResult> HandleResult(HttpRes response, JToken token, TJsonObject obj);

        protected virtual bool IsSuccessfulResponse(HttpRes response) => !response.HasError && response.StatusCode.IsSuccess();

        protected virtual bool IsSuccessfulJsonObject(HttpRes response, TJsonObject obj) => true;

        protected virtual bool IsSuccessfulJToken(HttpRes response, JToken token) => true;

        protected virtual OperateResult<TJsonObject> HandleJToken(HttpRes response, JToken token)
        {
            return token.ToObject<TJsonObject>()!;
        }

        protected virtual OperateResult<TResult> HandleUnsuccessfulResponse(HttpRes response)
        {
            base.PreCheckResponse(response);
            return OperateResult.CreateError<TResult>($"The response with status code {response.StatusCode} is unsuccessful: " + response.ResponseString.TruncateSafely(256), response.ExcuteTime);
        }

        protected virtual OperateResult<TResult> HandleUnsuccessfulJToken(HttpRes response, JToken token)
        {
            return OperateResult.CreateError<TResult>("The json is unsuccessful: " + token.ToString(Formatting.None).TruncateSafely(256), response.ExcuteTime);
        }

        protected virtual OperateResult<TResult> HandleUnsuccessfulJsonObject(HttpRes response, JToken token, TJsonObject obj)
        {
            return OperateResult.CreateError<TResult>("The result is unsuccessful: " + token.ToString().TruncateSafely(256), response.ExcuteTime);
        }

        protected virtual OperateResult<TResult> HandleNonJsonResult(HttpRes response)
        {
            return OperateResult.CreateError<TResult>("The response string is not a valid json: " + response.ResponseString.TruncateSafely(256), response.ExcuteTime);
        }

        protected virtual OperateResult<TResult> HandleEmptyResponse(HttpRes response)
        {
            return OperateResult.CreateError<TResult>("The response string is empty", response.ExcuteTime);
        }

        protected override async Task<OperateResult<TResult>> ExecuteInternalAsync(CancellationToken token = default)
        {
            var result = await base.ExecuteInternalAsync(token).DonotCapture();
            if (!result.Successful && !Client.IsOnline && LoginAndRetry)
            {
                await Client.FakeLogin(true, token).DonotCapture();
                result = await base.ExecuteInternalAsync(token).DonotCapture();
            }
            return result;
        }

        protected abstract override string Url { get; }
        protected abstract override HttpReqType ReqType { get; }
        protected virtual string? JsonResultPath { get; } = null;
        protected virtual bool LoginAndRetry { get; } = false;
    }

    public abstract class UserClientJsonAction<TClient, TResult> : UserClientJsonAction<TClient, TResult, TResult>
        where TClient : IUserClient
    {
        protected UserClientJsonAction(TClient client) : base(client)
        {
        }

        protected override OperateResult<TResult> HandleResult(HttpRes response, JToken token, TResult obj)
        {
            return OperateResult.CreateSuccess(obj, response.ExcuteTime);
        }

        protected sealed override OperateResult<TResult> HandleUnsuccessfulJsonObject(HttpRes response, JToken token, TResult obj)
        {
            return HandleUnsuccessfulResult(response, token, obj);
        }

        protected virtual OperateResult<TResult> HandleUnsuccessfulResult(HttpRes response, JToken token, TResult obj)
        {
            return base.HandleUnsuccessfulJsonObject(response, token, obj);
        }

        protected sealed override bool IsSuccessfulJsonObject(HttpRes response, TResult obj) => IsSuccessfulResult(response, obj);

        protected virtual bool IsSuccessfulResult(HttpRes response, TResult obj) => true;
    }
}
