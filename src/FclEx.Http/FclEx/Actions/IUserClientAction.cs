using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http;
using FclEx.Http.Core;
using FclEx.Utils;
using FclEx.Web.Core;

namespace FclEx.Actions
{
    public interface IUserClientAction<out TClient, T> : IHttpAction<T> where TClient : IUserClient
    {
        public TClient Client { get; }
        public ISession Session => Client.Session;
        public IUserAccount? Account => Client.Account;
        bool LoginAndRetry { get; }

        Task<OperateResult<T>> IHttpAction<T>.HandleResponseAsync(HttpRes response)
        {
            var time = ValueStopwatch.StartNew();
            var result = HandleResponse(response);
            return result.WithElapsed(time.GetElapsedTime() + response.ExcuteTime);
        }

        async Task<OperateResult<T>> IAbstractAction<T>.ExecuteInternalAsync(CancellationToken token)
        {
            var result = await ExecuteInternalAsyncBody(token).DonotCapture();
            if (!result.Successful && !Client.IsOnline && LoginAndRetry)
            {
                await Client.FakeLogin(true, token).DonotCapture();
                result = await ExecuteInternalAsyncBody(token).DonotCapture();
            }
            return result;
        }

        OperateResult<T> HandleResponse(HttpRes response)
        {
            if (IsOffline(response))
                return HandleOffline(response);

            if (!IsSuccessfulResponse(response))
                return HandleUnsuccessfulResponse(response);

            if (response.ResponseString.IsNullOrEmpty())
                return HandleEmptyResponse(response);

            return GetResult(response);
        }

        bool IsOffline(HttpRes response) => false;

        OperateResult<T> HandleOffline(HttpRes response)
        {
            Session.Offline();
            return "The client is offline now for unknown reason";
        }

        bool IsSuccessfulResponse(HttpRes response) => !response.HasError && response.StatusCode.IsSuccess();

        OperateResult<T> HandleUnsuccessfulResponse(HttpRes response)
        {
            return response.HasError
                ? OperateResult.CreateError(response.Exception!)
                : $"The response with status code {response.StatusCode} is unsuccessful: "
                  + response.ResponseString.TruncateSafely(256);
        }

        OperateResult<T> HandleEmptyResponse(HttpRes response)
        {
            return "The response string is empty";
        }

        OperateResult<T> GetResult(HttpRes response);
    }
}
