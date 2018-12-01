using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Helpers;
using FclEx.Http.Event;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FclEx.Http.Actions
{
    public abstract class AbstractAction : IAction
    {
        protected static ActionEventListener NullListener { get; } = (sender, @event) => @event.ToValueTask();
        protected string ActionName => GetType().GetDescription();
        protected virtual int MaxReTryTimes { get; set; } = 3;
        protected int ExcuteTimes { get; set; }
        protected int ErrorTimes { get; set; }
        protected ActionEventListener Listener { get; }
        public ILogger Logger { get; }

        protected AbstractAction(ILogger logger = null, ActionEventListener listener = null)
        {
            Listener = listener ?? NullListener;
            Logger = logger ?? NullLogger.Instance;
        }

        protected virtual void LogActionEvent(ActionEvent actionEvent)
        {
            if (!Debugger.IsLogging()) return;

            var type = actionEvent.Type;
            var typeName = type.GetDescription();
            var target = actionEvent.Target;

            switch (type)
            {
                case ActionEventType.EvtError:
                {
                    var ex = (Exception)target;
                    var msg = ex.ToString().TrimEnd();
                    Logger.LogTrace($"[Action={ActionName}, Result={typeName}, {msg}]");
                    break;
                }

                case ActionEventType.EvtRetry:
                {
                    var ex = (Exception)target;
                    Logger.LogTrace($"[Action={ActionName}, Result={typeName}, ErrorTimes={ErrorTimes}][{ex}]");
                    break;
                }

                case ActionEventType.EvtCanceled:
                    Logger.LogTrace($"[Action={ActionName}, Result={typeName}, Target={target}]");
                    break;

                default:
                    Logger.LogTrace($"[Action={ActionName}, Result={typeName}]");
                    break;
            }
        }

        protected virtual ValueTask<ActionEvent> NotifyActionEventAsync(ActionEvent actionEvent)
        {
            try
            {
                LogActionEvent(actionEvent);
                return Listener(this, actionEvent);
            }
            catch (Exception ex)
            {
                return HandleExceptionAsync(ex);
            }
        }

        protected virtual ValueTask<ActionEvent> HandleExceptionAsync(Exception ex)
        {
            ++ErrorTimes;
            try
            {
                var @event = ActionEvent.Create(ErrorTimes < MaxReTryTimes ?
                    ActionEventType.EvtRetry : ActionEventType.EvtError, ex);
                LogActionEvent(@event);
                return Listener(this, @event);
            }
            catch (Exception e)
            {
                throw new Exception($"throw an unhandled exception when excute [{nameof(HandleExceptionAsync)}] method.", e);
            }
        }

        protected ValueTask<ActionEvent> NotifyActionEventAsync(ActionEventType type, object target = null)
            => NotifyActionEventAsync(ActionEvent.Create(type, target));

        protected ValueTask<ActionEvent> NotifyOkEventAsync(object target = null)
            => NotifyActionEventAsync(ActionEventType.EvtOk, target);

        protected ValueTask<ActionEvent> NotifyRetryEventAsync(Exception ex) => NotifyActionEventAsync(ActionEventType.EvtRetry, ex);
        protected ValueTask<ActionEvent> NotifyRetryEventAsync(string msg = null) => NotifyActionEventAsync(ActionEventType.EvtRetry, CreateEx(msg));
        protected ValueTask<ActionEvent> NotifyCancelEventAsync() => NotifyActionEventAsync(ActionEventType.EvtCanceled, this);
        protected ValueTask<ActionEvent> NotifyErrorAsync(Exception ex) => NotifyActionEventAsync(ActionEvent.Create(ActionEventType.EvtError, ex));
        protected ValueTask<ActionEvent> NotifyErrorAsync(string msg = null) => NotifyErrorAsync(CreateEx(msg));
        protected ValueTask<ActionEvent> NotifyObjectErrorAsync<T>(T obj, string msg = null, Exception innerException = null)
            => NotifyErrorAsync(ObjectException.Create(obj, msg, innerException));

        private static Exception CreateEx(string msg) => new SimpleException(msg);

        protected abstract ValueTask<ActionEvent> ExecuteInternalAsync(CancellationToken token);

        public async ValueTask<ActionEvent> ExecuteAsync(CancellationToken token)
        {
            if (Logger.IsEnabled(LogLevel.Trace))
            {
                Logger.LogTrace($"[Action={ActionName} Begin]");
                var watch = new Stopwatch();
                watch.Start();
                var result = await ExecuteInternalAsync(token).DonotCapture();
                watch.Stop();
                Logger.LogTrace($"[Action={ActionName} End, ResultType={result.Type.GetDescription()}. {watch.ElapsedMilliseconds} ms]");
                return result;
            }
            else
            {
                return await ExecuteInternalAsync(token).DonotCapture();
            }
        }
    }
}
