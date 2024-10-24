namespace FclEx.Utils;

public class AsyncEventHandlerTests
{
    private class TestModel
    {
        public SafeCounter Counter { get; } = new();

        public event AsyncEventHandler<TestModel, TestModel> OnNotify = async (sender, tester) =>
        {
            await Task.Yield();
            sender.Counter.Increment();
        };

        public Task Notify()
        {
            return OnNotify.InvokeAsync(this, this);
        }
    }

    [Fact]
    public async Task Test()
    {
        var tester = new TestModel();
        tester.OnNotify += async (sender, e) =>
        {
            await Task.Yield();
            sender.Counter.Increment();
        };

        tester.OnNotify += async (sender, e) =>
        {
            await Task.Yield();
            sender.Counter.Increment();
        };

        await tester.Notify();

        Assert.Equal(3, tester.Counter.Value);
    }
}