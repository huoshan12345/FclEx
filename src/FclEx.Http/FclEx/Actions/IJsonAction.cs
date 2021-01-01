using FclEx.Http.Core;
using FclEx.Utils;
using Newtonsoft.Json.Linq;

namespace FclEx.Actions
{
    public interface IJsonAction<T>
    {
        string? JsonResultPath { get; }

        OperateResult<T> GetResult(HttpRes response)
        {
            var (successful, _, json, ex) = GetJsonString(response);
            if (!successful)
                return ex!;

            var token = json!.ToJToken();
            var resultToken = JsonResultPath == null
                ? token
                : token.SelectToken(JsonResultPath);

            return GetResult(response, json!, token, resultToken!);
        }

        OperateResult<string> GetJsonString(HttpRes response)
        {
            var str = response.ResponseString;
            return str.IsPossibleJson()
                ? OperateResult.CreateSuccess(response.ResponseString)
                : OperateResult.CreateError<string>("The response string is not a valid json: " + str.TruncateSafely(256));
        }

        StringError GetJTokenError(HttpRes response, string json, JToken token, JToken? resultToken)
        {
            if (resultToken == null)
            {
                const string msg = "The result object does not exist in json";
                var error = JsonResultPath == null ? msg : msg + " at " + JsonResultPath;
                error = error + ": " + json.TruncateSafely(256);
                return (true, error);
            }
            return (false, string.Empty);
        }

        OperateResult<T> GetResult(HttpRes response, string json, JToken token, JToken? resultToken)
        {
            var (hasError, error) = GetJTokenError(response, json, token, resultToken);
            if (hasError)
                return error;
            return resultToken!.ToObject<T>()!;
        }
    }

    public interface IJsonAction : IJsonAction<Unit>
    {
        OperateResult<Unit> IJsonAction<Unit>.GetResult(HttpRes response, string json, JToken token, JToken? resultToken)
            => OperateResult.Success;
    }
}
