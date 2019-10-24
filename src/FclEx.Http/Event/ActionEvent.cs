using System;
using System.Threading.Tasks;
using FclEx.Utils;
using Newtonsoft.Json;

namespace FclEx.Http.Event
{
    /// <summary>
    /// 用来表示一个action的执行结果
    /// </summary>
    public struct ActionEvent : IActionEvent
    {
        [JsonIgnore] public Exception Exception => IsError ? (Exception)Target : null;
        [JsonIgnore] public string ExMsg => Exception?.Message;
        [JsonIgnore] public bool IsOk => Type == ActionEventType.EvtOk;
        [JsonIgnore] public bool IsError => Type == ActionEventType.EvtError;
        [JsonIgnore] public bool IsRetry => Type == ActionEventType.EvtRetry;

        public ActionEventType Type { get; }

        [JsonIgnore]
        public object Target { get; }

        public static ActionEvent Repeat() => new ActionEvent(ActionEventType.EvtRepeat, null);
        public static ActionEvent Create(ActionEventType type, object target) => new ActionEvent(type, target);
        public static ActionEvent Ok(object target) => Create(ActionEventType.EvtOk, target);
        public static ActionEvent Error(Exception ex) => Create(ActionEventType.EvtError, ex);
        public static ActionEvent Error(string msg) => Create(ActionEventType.EvtError, new SimpleException(msg));
        public static ActionEvent Cancel(object target) => Create(ActionEventType.EvtCanceled, target);
        public static ActionEvent<T> Ok<T>(T target) => new ActionEvent<T>(ActionEventType.EvtOk, target);
        public static ActionEvent<T> Error<T>(Exception ex) => new ActionEvent<T>(ActionEventType.EvtError, ex);
        public static ActionEvent<T> Error<T>(string msg) => new ActionEvent<T>(ActionEventType.EvtError, new SimpleException(msg));
        public ActionEvent<T> ToExplicit<T>() => new ActionEvent<T>(Type, Target);
        public static implicit operator ActionEvent(Exception ex) => Error(ex);
        public static implicit operator Task<ActionEvent>(ActionEvent actionEvent) => actionEvent.ToTask();
        public static implicit operator ValueTask<ActionEvent>(ActionEvent actionEvent) => actionEvent.ToValueTask();

        public override string ToString() => $"{Type.GetFullDescription()}, target={Target ?? ""}]";

        public ActionEvent(ActionEventType type, object target)
        {
            Type = type;
            Target = target;
        }

        [JsonIgnore]
        public static ActionEvent EmptyOkEvent { get; } = new ActionEvent(ActionEventType.EvtOk, null);

        public static implicit operator OperateResult(ActionEvent actionEvent)
        {
            switch (actionEvent.Type)
            {
                case ActionEventType.EvtOk: return OperateResult.CreateSuccess();
                case ActionEventType.EvtError: return OperateResult.CreateError(actionEvent.Exception);
                case ActionEventType.EvtCanceled: return OperateResult.CreateCancel();
                case ActionEventType.EvtRetry:
                case ActionEventType.EvtRepeat:
                    return OperateResult.CreateError(OperateResultCodes.NotFinished, "the operate was not finished");
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public OperateResult ToOperateResult() => this;
    }
}
