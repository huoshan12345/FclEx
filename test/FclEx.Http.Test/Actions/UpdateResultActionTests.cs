using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Actions;
using FclEx.Http.Event;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FclEx.Http.Test.Actions
{
    public class UpdateResultActionTests
    {
        public class TestAction : AbstractAction
        {
            private int _times;
            public int Times => _times;

            public TestAction(ILogger logger = null, ActionEventListener listener = null)
                : base(logger, listener)
            {
            }


            protected override ValueTask<ActionEvent> ExecuteInternalAsync(CancellationToken token)
            {
                Interlocked.Increment(ref _times);
                return ActionEvent.Ok((object)Times).ToValueTask();
            }
        }

        [Fact]
        public async Task Test()
        {
            var result = await new TestAction().Repeat(e => e.True<int>(i => i < 3)).ExecuteAutoAsync();
            Assert.Equal(3, result.ToExplicit<int>().Result);
        }
    }
}
