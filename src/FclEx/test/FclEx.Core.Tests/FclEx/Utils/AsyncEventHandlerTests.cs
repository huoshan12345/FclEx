namespace FclEx.Utils;

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
                var span = TimeSpan.FromMilliseconds(600);
                await Task.Delay(span);
                helper.WriteLine(span.ToString());
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
            var span = TimeSpan.FromMilliseconds(100);
            await Task.Delay(span);
            _helper.WriteLine(span.ToString());
        };

        tester.OnNotify += async (sender, e) =>
        {
            var span = TimeSpan.FromMilliseconds(300);
            await Task.Delay(span);
            _helper.WriteLine(span.ToString());
        };

        await tester.Notify();
        _helper.WriteLine("Notify");
    }
}