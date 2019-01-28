using System;
using System.Threading.Tasks;
using FclEx.Http.Event;
using FclEx.Utils;

namespace FclEx.Http
{
    public static class ActionEventExtensions
    {
        public static bool IsOk(this IActionEvent @event) => @event.Type == ActionEventType.EvtOk;

        public static bool IsError(this IActionEvent @event) => @event.Type == ActionEventType.EvtError;

        public static bool IsRetry(this IActionEvent @event) => @event.Type == ActionEventType.EvtRetry;

        public static bool TryGet<T>(this IActionEvent e, out T result)
        {
            if (e.IsOk() && e.Target is T r)
            {
                result = r;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static bool True<T>(this IActionEvent @event, Func<T, bool> predicate)
        {
            return @event.TryGet<T>(out var result) && predicate(result);
        }

        public static bool TryGetFromObjEx<T>(this IActionEvent e, out T result)
        {
            if (e.IsError() && e.Target is ObjectException<T> ex)
            {
                result = ex.Target;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static bool TrueFromObjEx<T>(this IActionEvent @event, Func<T, bool> predicate)
        {
            return @event.TryGetFromObjEx<T>(out var result) && predicate(result);
        }

        public static Exception GetEx(this IActionEvent @event) => @event.IsError() ? (Exception)@event.Target : null;

        public static string GetExMsg(this IActionEvent @event) => @event.GetEx()?.Message;
    }
}
