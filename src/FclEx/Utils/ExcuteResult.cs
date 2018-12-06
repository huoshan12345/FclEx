using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FclEx.Utils
{
    public class ExcuteResult
    {
        public bool Successful => Code == 0;
        public int Code { get; }

        [JsonIgnore]
        public Exception Exception { get; }
        public TimeSpan Elapsed { get; protected set; }
        public string Msg => Exception?.Message;
        public string StackTrace => Exception?.StackTrace;

        public static ExcuteResult Success { get; } = new ExcuteResult(TimeSpan.Zero);

        public static ExcuteResult NotImplemented { get; }
            = new ExcuteResult(-2, new NotImplementedException("this function is not implemented"));

        internal ExcuteResult(int code, Exception ex)
        {
            Code = Check.NotEqual(code, 0, nameof(code));
            Exception = Check.NotNull(ex, nameof(ex));
            Elapsed = default;
        }

        internal ExcuteResult(int code, string msg, string stackTrace = null)
            : this(code, msg == null ? null : new SimpleException(msg, stackTrace))
        {
        }

        internal ExcuteResult(TimeSpan elapsed)
        {
            Code = 0;
            Exception = null;
            Elapsed = elapsed;
        }

        public ExcuteResult<T> ToExplicit<T>()
        {
            if (Successful)
                throw new InvalidOperationException("cannot convert to explicit when result is successful");
            else
                return new ExcuteResult<T>(Code, Exception);
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
            return CreateError(-1, error);
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
            return CreateError<T>(-1, error);
        }

        public static implicit operator ExcuteResult(Exception ex)
        {
            return new ExcuteResult(-1, ex);
        }

        public static implicit operator ExcuteResult(string error)
        {
            return new ExcuteResult(-1, error, null);
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
    }

    public class ExcuteResult<T> : ExcuteResult
    {
        public T Result { get; }

        internal ExcuteResult(int code, Exception ex) : base(code, ex)
        {
            Result = default;
        }

        internal ExcuteResult(T result, TimeSpan elapsed) : base(elapsed)
        {
            Result = result;
        }

        public static implicit operator ExcuteResult<T>(Exception ex)
        {
            return new ExcuteResult<T>(-1, ex);
        }

        public static implicit operator ExcuteResult<T>(string error)
        {
            return new ExcuteResult<T>(-1, new SimpleException(error));
        }

        public static implicit operator ExcuteResult<T>(T item)
        {
            return item == null
                ? ExcuteResult.CreateError<T>(-1, "结果为空")
                : ExcuteResult.CreateSuccess(item, TimeSpan.Zero);
        }
    }
}
