using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Actions;
using FclEx.Utils;
using Xunit;

namespace FclEx.Http.Test.Actions
{
    public class ActionFutureTests
    {
        public class Actor : IActor
        {
            private readonly bool _error;
            private readonly bool _throw;

            public int ExecuteTimes { get; private set; }

            public Actor(bool error, bool @throw)
            {
                _error = error;
                _throw = @throw;
            }

            public Task<IOperateResult> ExecuteAsync(CancellationToken token = default)
            {
                ExecuteTimes++;
                if (_throw) throw new InvalidOperationException();
                return ((IOperateResult)(_error ? OperateResult.CreateError("Error") : OperateResult.Success)).ToTask();
            }
        }

        [Fact]
        public async Task PushAction_NoError()
        {
            var future = new ActionFuture();
            var actions = Enumerable.Range(1, 3)
                .Select(m => new Actor(false, false))
                .ToList();

            foreach (var action in actions)
            {
                future.PushAction(action);
            }

            var r = await future.ExecuteAsync();
            Assert.True(r.Successful);
            Assert.True(actions.All(m => m.ExecuteTimes == 1));
        }

        [Fact]
        public async Task PushAction_StopOnError()
        {
            var future = new ActionFuture();
            var actions = new[]
            {
                new Actor(false, false),
                new Actor(true, false),
                new Actor(false, false)
            };

            foreach (var action in actions)
            {
                future.PushAction(action);
            }

            var r = await future.ExecuteAsync();
            Assert.False(r.Successful);
            Assert.IsType<SimpleException>(r.Exception);

            Assert.True(actions[0].ExecuteTimes == 1);
            Assert.True(actions[1].ExecuteTimes == 1);
            Assert.True(actions[2].ExecuteTimes == 0);
        }

        [Fact]
        public async Task PushAction_StopOnException()
        {
            var future = new ActionFuture();
            var actions = new[]
            {
                new Actor(false, false),
                new Actor(false, true),
                new Actor(false, false)
            };

            foreach (var action in actions)
            {
                future.PushAction(action);
            }

            var r = await future.ExecuteAsync();
            Assert.False(r.Successful);
            Assert.IsType<InvalidOperationException>(r.Exception);

            Assert.True(actions[0].ExecuteTimes == 1);
            Assert.True(actions[1].ExecuteTimes == 1);
            Assert.True(actions[2].ExecuteTimes == 0);
        }

        [Fact]
        public async Task PushAction_DonotStopOnError()
        {
            var future = new ActionFuture();
            var actions = new[]
            {
                new Actor(false, false),
                new Actor(true, false),
                new Actor(false, false)
            };

            foreach (var action in actions)
            {
                future.PushAction(rs => action, r => false);
            }

            var result = await future.ExecuteAsync();
            Assert.True(result.Successful);
            Assert.True(actions.All(m => m.ExecuteTimes == 1));
        }


        [Fact]
        public async Task PushAction_DonotStopOnException()
        {
            var future = new ActionFuture();
            var actions = new[]
            {
                new Actor(false, false),
                new Actor(false, true),
                new Actor(false, false)
            };

            foreach (var action in actions)
            {
                future.PushAction(rs => action, r => false);
            }

            var result = await future.ExecuteAsync();
            Assert.True(result.Successful);
            Assert.True(actions.All(m => m.ExecuteTimes == 1));
        }
    }
}
