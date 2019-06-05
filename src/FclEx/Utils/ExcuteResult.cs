using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FclEx.Helpers;
using Newtonsoft.Json;
using OperateResult = FclEx.Utils.IOperateResult<FclEx.Utils.IUnit>;

namespace FclEx.Utils
{
    [Obsolete("使用" + nameof(OperateResult))]
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
            = new ExcuteResult(ExcuteResultCodes.NotImplemented, TimeSpan.Zero,
                new NotImplementedException("this function is not implemented"));

        internal ExcuteResult(int code, TimeSpan elapsed, Exception ex)
        {
            Code = Check.NotEqual(code, ExcuteResultCodes.Success, nameof(code));
            Exception = Check.NotNull(ex, nameof(ex));
            Elapsed = elapsed;
        }

        internal ExcuteResult(int code, TimeSpan elapsed, string msg, string stackTrace = null)
            : this(code, elapsed, new SimpleException(msg.GetOrEmpty(), stackTrace))
        {
        }

        internal ExcuteResult(TimeSpan elapsed)
        {
            Code = ExcuteResultCodes.Success;
            Exception = null;
            Elapsed = elapsed;
        }

        public ExcuteResult<T> ToExplicit<T>()
        {
            if (Successful)
                throw new InvalidOperationException("cannot convert to explicit when result is successful");
            else
                return new ExcuteResult<T>(Code, Elapsed, Exception);
        }

        public static ExcuteResult CreateSuccess(TimeSpan elapsed) => new ExcuteResult(elapsed);

        public static ExcuteResult CreateError(int code, string error) => new ExcuteResult(code, TimeSpan.Zero, error);

        public static ExcuteResult CreateError(string error) => CreateError(ExcuteResultCodes.FromString, error);

        public static ExcuteResult CreateError(Exception ex, TimeSpan elapsed = default) => new ExcuteResult(ExcuteResultCodes.FromException, elapsed, ex);

        public static ExcuteResult<T> CreateSuccess<T>(T item) => CreateSuccess(item, TimeSpan.Zero);

        public static ExcuteResult<T> CreateSuccess<T>(T item, TimeSpan elapsed) => new ExcuteResult<T>(item, elapsed);

        public static ExcuteResult<T> CreateError<T>(int code, string error) => new ExcuteResult<T>(code, TimeSpan.Zero, new SimpleException(error));

        public static ExcuteResult<T> CreateError<T>(string error) => CreateError<T>(ExcuteResultCodes.FromString, error);

        public static ExcuteResult<T> CreateError<T>(Exception ex, TimeSpan elapsed = default) => new ExcuteResult<T>(ExcuteResultCodes.FromException, elapsed, ex);

        public static implicit operator ExcuteResult(Exception ex) => CreateError(ex);

        public static implicit operator ExcuteResult(string error) => new ExcuteResult(ExcuteResultCodes.FromString, TimeSpan.Zero, error, null);

        public static ExcuteResult Excute(Action action)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                action();
                return CreateSuccess(watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return CreateError(ex, watch.GetElapsedTime());
            }
        }

        public static ExcuteResult<T> Excute<T>(Func<T> action)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                var result = action();
                return CreateSuccess(result, watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return CreateError<T>(ex, watch.GetElapsedTime());
            }
        }

        public static ExcuteResult Excute(Func<ExcuteResult> action) => Excute<ExcuteResult>(action).Unwrap();

        public static ExcuteResult<T> Excute<T>(Func<ExcuteResult<T>> action) => Excute<ExcuteResult<T>>(action).Unwrap();

        public static async Task<ExcuteResult> ExcuteAsync(Func<Task> action)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                await action().DonotCapture();
                return CreateSuccess(watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return CreateError(ex, watch.GetElapsedTime());
            }
        }

        public static async Task<ExcuteResult<T>> ExcuteAsync<T>(Func<Task<T>> action)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                var result = await action().DonotCapture();
                return CreateSuccess(result, watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return CreateError<T>(ex, watch.GetElapsedTime());
            }
        }

        public static async Task<ExcuteResult> ExcuteAsync(Func<Task<ExcuteResult>> action)
            => (await ExcuteAsync<ExcuteResult>(action)).Unwrap();

        public static async Task<ExcuteResult<T>> ExcuteAsync<T>(Func<Task<ExcuteResult<T>>> action)
            => (await ExcuteAsync<ExcuteResult<T>>(action)).Unwrap();

        public static async ValueTask<ExcuteResult<T>> ExcuteValueAsync<T>(Func<ValueTask<T>> action)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                var result = await action().DonotCapture();
                return CreateSuccess(result, watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return CreateError<T>(ex, watch.GetElapsedTime());
            }
        }

        public static async ValueTask<ExcuteResult> ExcuteValueAsync(Func<ValueTask<ExcuteResult>> action)
            => (await ExcuteValueAsync<ExcuteResult>(action)).Unwrap();

        public static async ValueTask<ExcuteResult> ExcuteValueAsync(Func<ValueTask> action)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                await action().DonotCapture();
                return CreateSuccess(watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return CreateError(ex, watch.GetElapsedTime());
            }
        }

        public static async ValueTask<ExcuteResult<T>> ExcuteValueAsync<T>(Func<ValueTask<ExcuteResult<T>>> action)
            => (await ExcuteValueAsync<ExcuteResult<T>>(action)).Unwrap();

        public static void ExcuteBg(Action action) => TaskHelper.RunBg(() => Excute(action));
        public static void ExcuteBg<T>(Func<T> action) => TaskHelper.RunBg(() => Excute(action));
        public static void ExcuteBg(Func<ExcuteResult> action) => TaskHelper.RunBg(() => Excute(action));
        public static void ExcuteBg<T>(Func<ExcuteResult<T>> action) => TaskHelper.RunBg(() => Excute(action));

        public static void ExcuteBgAsync(Func<Task> action) => TaskHelper.RunBg(() => ExcuteAsync(action));
        public static void ExcuteBgAsync<T>(Func<Task<T>> action) => TaskHelper.RunBg(() => ExcuteAsync(action));
        public static void ExcuteBgAsync(Func<Task<ExcuteResult>> action) => TaskHelper.RunBg(() => ExcuteAsync(action));
        public static void ExcuteBgAsync<T>(Func<Task<ExcuteResult<T>>> action) => TaskHelper.RunBg(() => ExcuteAsync(action));

        public static void ExcuteBgValueAsync(Func<ValueTask> action) => TaskHelper.RunBg(() => ExcuteValueAsync(action));
        public static void ExcuteBgValueAsync<T>(Func<ValueTask<T>> action) => TaskHelper.RunBg(() => ExcuteValueAsync(action));
        public static void ExcuteBgValueAsync(Func<ValueTask<ExcuteResult>> action) => TaskHelper.RunBg(() => ExcuteValueAsync(action));
        public static void ExcuteBgValueAsync<T>(Func<ValueTask<ExcuteResult<T>>> action) => TaskHelper.RunBg(() => ExcuteValueAsync(action));
    }
}
