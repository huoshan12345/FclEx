using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using FclEx.Http.Event;
using FclEx.Utils;

namespace FclEx.Http.Actions
{
    public class ActionFuture : IActionFuture
    {
        private readonly List<Func<object[], IActor>> _queue = new List<Func<object[], IActor>>();

        public virtual async Task<ActionEvent> ExecuteAsync(CancellationToken token)
        {
            var results = new object[_queue.Count];
            var actions = new IActor[_queue.Count];
            var lastEvent = ActionEvent.EmptyOkEvent;
            for (var i = 0; i < _queue.Count; i++)
            {
                if (token.IsCancellationRequested)
                    return ActionEvent.Cancel(this);

                actions[i] = actions[i] ?? _queue[i](results); // action只生成一次
                var action = actions[i];
                if (action == null) continue;

                var result = await action.ExecuteAutoAsync(token).DonotCapture();
                results[i] = result.Target;
                switch (result.Type)
                {
                    case ActionEventType.EvtError:
                    case ActionEventType.EvtCanceled:
                        return result;

                    case ActionEventType.EvtRetry:
                    case ActionEventType.EvtRepeat:
                        --i; // 回退，即重复执行
                        break;

                    default:
                        lastEvent = result;
                        break;
                }
            }
            return lastEvent;
        }

        public int Count => _queue.Count;

        public IActionFuture PushAction(Func<object[], IActor> func)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            _queue.Add(func);
            return this;
        }
    }
}
