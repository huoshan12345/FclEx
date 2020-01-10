using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using FclEx.Utils;

namespace FclEx.Actions
{
    public class ActionFuture : IActionFuture
    {
        private readonly List<MetaData> _queue = new List<MetaData>();
        private readonly bool _stopOnError;

        public ActionFuture(bool stopOnError = true)
        {
            _stopOnError = stopOnError;
        }

        public virtual async Task<IOperateResult> ExecuteAsync(CancellationToken token = default)
        {
            var watch = ValueStopwatch.StartNew();
            var results = new IOperateResult[_queue.Count];
            var actions = new IActor[_queue.Count];
            var lastEvent = (IOperateResult)OperateResult.Success;
            for (var i = 0; i < _queue.Count; i++)
            {
                if (token.IsCancellationRequested)
                    return OperateResult.Cancel;

                actions[i] = actions[i] ?? _queue[i].ActorSelector(results); // action只生成一次
                var action = actions[i];
                if (action == null)
                    continue;

                var result = await OperateResult.ExcuteAsync(() => action.ExecuteAsync(token))
                    .DonotCapture();

                results[i] = result;
                lastEvent = result;

                var termination = _queue[i].TerminationCondition;
                if (termination == null)
                {
                    if (_stopOnError && result.HasError())
                        break;
                }
                else if (termination(result))
                {
                    break;
                }
            }
            return lastEvent.WithElapsed(watch.GetElapsedTime());
        }

        public int Count => _queue.Count;

        public IActionFuture PushAction(Func<IOperateResult[], IActor> actorSelector, Func<IOperateResult, bool> terminationCondition = null)
        {
            Guard.Argument(actorSelector, nameof(actorSelector)).NotNull();
            _queue.Add(new MetaData(actorSelector, terminationCondition));
            return this;
        }

        private readonly struct MetaData
        {
            public MetaData(Func<IOperateResult[], IActor> actorSelector, Func<IOperateResult, bool> terminationCondition)
            {
                ActorSelector = actorSelector;
                TerminationCondition = terminationCondition;
            }

            public Func<IOperateResult[], IActor> ActorSelector { get; }
            public Func<IOperateResult, bool> TerminationCondition { get; }
        }
    }
}
