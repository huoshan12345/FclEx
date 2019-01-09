using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FclEx.Utils
{
    public struct ExcuteResult : IExcuteResult
    {
        public bool Successful => Code == ExcuteResultCodes.Success;
        public int Code { get; }
        [JsonIgnore]
        public Exception Exception { get; }
        public TimeSpan Elapsed { get; }
        public string Msg => Exception?.Message;
        public string StackTrace => Exception?.StackTrace;

        public static ExcuteResult Success { get; } = new ExcuteResult(TimeSpan.Zero);

        public static ExcuteResult NotImplemented { get; }
            = new ExcuteResult(ExcuteResultCodes.NotImplemented,
                new NotImplementedException("this function is not implemented"));

        internal ExcuteResult(int code, Exception ex)
        {
            Code = Check.NotEqual(code, ExcuteResultCodes.Success, nameof(code));
            Exception = Check.NotNull(ex, nameof(ex));
            Elapsed = default;
        }

        internal ExcuteResult(int code, string msg, string stackTrace = null)
            : this(code, msg == null ? null : new SimpleException(msg, stackTrace))
        {
        }

        internal ExcuteResult(TimeSpan elapsed)
        {
            Code = ExcuteResultCodes.Success;
            Exception = null;
            Elapsed = elapsed;
        }

        public static ExcuteResult CreateSuccess(TimeSpan elapsed)
        {
            return new ExcuteResult(elapsed);
        }

        public static ExcuteResult CreateError(int code, string error)
        {
            return new ExcuteResult(code, error);
        }

        public static ExcuteResult CreateError(string error)
        {
            return CreateError(ExcuteResultCodes.FromString, error);
        }

        public static ExcuteResult<T> CreateSuccess<T>(T item, TimeSpan elapsed)
        {
            return new ExcuteResult<T>(item, elapsed);
        }

        public static ExcuteResult<T> CreateError<T>(int code, string error)
        {
            return new ExcuteResult<T>(code, new SimpleException(error));
        }

        public static ExcuteResult<T> CreateError<T>(string error)
        {
            return CreateError<T>(ExcuteResultCodes.FromString, error);
        }

        public static implicit operator ExcuteResult(Exception ex)
        {
            return new ExcuteResult(ExcuteResultCodes.FromException, ex);
        }

        public static implicit operator ExcuteResult(string error)
        {
            return new ExcuteResult(ExcuteResultCodes.FromString, error, null);
        }

        public static ExcuteResult Excute(Action action)
        {
            try
            {
                var watch = ValueStopwatch.StartNew();
                action();
                return CreateSuccess(watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public static async Task<ExcuteResult> ExcuteAsync(Func<Task> action)
        {
            try
            {
                var watch = ValueStopwatch.StartNew();
                await action().DonotCapture();
                return CreateSuccess(watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public static async ValueTask<ExcuteResult> ExcuteValueAsync(Func<ValueTask> action)
        {
            try
            {
                var watch = ValueStopwatch.StartNew();
                await action().DonotCapture();
                return CreateSuccess(watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public static ExcuteResult<T> Excute<T>(Func<T> action)
        {
            try
            {
                var watch = ValueStopwatch.StartNew();
                var result = action();
                return ExcuteResult.CreateSuccess(result, watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public static ExcuteResult<T> Excute<T>(Func<ExcuteResult<T>> action) 
            => Excute<ExcuteResult<T>>(action).Unwrap();

        public static async Task<ExcuteResult<T>> ExcuteAsync<T>(Func<Task<T>> action)
        {
            try
            {
                var watch = ValueStopwatch.StartNew();
                var result = await action().DonotCapture();
                return ExcuteResult.CreateSuccess(result, watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public static async Task<ExcuteResult<T>> ExcuteAsync<T>(Func<Task<ExcuteResult<T>>> action)
            =>(await ExcuteAsync<ExcuteResult<T>>(action)).Unwrap();

        public static async ValueTask<ExcuteResult<T>> ExcuteValueAsync<T>(Func<ValueTask<T>> action)
        {
            try
            {
                var watch = ValueStopwatch.StartNew();
                var result = await action().DonotCapture();
                return ExcuteResult.CreateSuccess(result, watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public static async ValueTask<ExcuteResult<T>> ExcuteValueAsync<T>(Func<ValueTask<ExcuteResult<T>>> action)
            => (await ExcuteValueAsync<ExcuteResult<T>>(action)).Unwrap();
    }
}
