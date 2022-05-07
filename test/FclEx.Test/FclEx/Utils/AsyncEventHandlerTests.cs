using System;
using System.Threading.Tasks;
using FclEx.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Utils
{
    public class AsyncEventHandlerTests
    {
        private readonly ITestOutputHelper _helper;

        public AsyncEventHandlerTests(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        private class Tester
        {
            public Tester(ITestOutputHelper helper)
            {
                OnNotify += async (sender, tester) =>
                {
                    await TaskHelper.Delay(5);
                    helper.WriteLine("default");
                };
            }

            public event AsyncEventHandler<Tester, Tester> OnNotify = (sender, args) => Task.CompletedTask;

            public Task Notify()
            {
                return OnNotify.InvokeAsync(this, this);
            }
        }

        [Fact]
        public async Task Test()
        {
            var tester = new Tester(_helper);
            tester.OnNotify += async (sender, e) =>
            {
                await TaskHelper.Delay(1);
                _helper.WriteLine("1 seconds");
            };

            tester.OnNotify += async (sender, e) =>
            {
                await TaskHelper.Delay(2);
                _helper.WriteLine("2 seconds");
            };

            await tester.Notify();
            _helper.WriteLine("Notify");

            await TaskHelper.Delay(10);
        }
    }
}
