using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FclEx.Utils
{
    public struct ExcuteResult
    {
        public bool Successful => Code == 0;
        public int Code { get; }
        public Exception Exception { get; }
        public TimeSpan Elapsed { get; }

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
                return ExcuteResult<T>.CreateSuccess(result, watch.GetElapsedTime());
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
                return ExcuteResult<T>.CreateSuccess(result, watch.GetElapsedTime());
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
                return ExcuteResult<T>.CreateSuccess(result, watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }

    public struct ExcuteResult<T>
    {
        public bool Successful => Code == 0;
        public int Code { get; }
        public Exception Exception { get; }
        public T Result { get; }
        public TimeSpan Elapsed { get; }

        internal ExcuteResult(int code, Exception ex)
        {
            Code = Check.NotEqual(code, 0, nameof(code));
            Exception = Check.NotNull(ex, nameof(ex));
            Elapsed = TimeSpan.Zero;
            Result = default;
        }

        internal ExcuteResult(T result, TimeSpan elapsed)
        {
            Result = result;
            Code = 0;
            Exception = null;
            Elapsed = elapsed;
        }

        public static implicit operator ExcuteResult(ExcuteResult<T> result)
        {
            if (result.Successful)
                return new ExcuteResult(result.Elapsed);
            else
            {
                return new ExcuteResult(result.Code, result.Exception);
            }
        }

        public static implicit operator ExcuteResult<T>(ExcuteResult result)
        {
            return result.ToExplicit<T>();
        }

        public static implicit operator ExcuteResult<T>(Exception ex)
        {
            return new ExcuteResult<T>(-1, ex);
        }

        public static implicit operator ExcuteResult<T>(string error)
        {
            return new ExcuteResult<T>(-1, new SimpleException(error));
        }

        public static ExcuteResult<T> CreateSuccess(T item, TimeSpan elapsed)
        {
            return new ExcuteResult<T>(item, elapsed);
        }

        public static ExcuteResult<T> CreateError(int code, string error)
        {
            return new ExcuteResult<T>(code, new SimpleException(error));
        }

        public static ExcuteResult<T> CreateError(string error)
        {
            return CreateError(-1, error);
        }
    }
}
