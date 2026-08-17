namespace FclEx.Extensions;

public class AsyncEventHandlerExtensionsTests
{
    [Fact]
    public void GetInvocationList_ReturnsAListWithTheRequestedDelegateType()
    {
        AsyncEventHandler<object> first = _ => Task.CompletedTask;
        AsyncEventHandler<object> second = _ => Task.CompletedTask;
        var handler = first + second;

        var result = handler.GetInvocationList<AsyncEventHandler<object>>();

        Assert.Collection(result,
            item => Assert.Same(first, item),
            item => Assert.Same(second, item));
    }

    [Fact]
    public void GetInvocationList_IsASnapshotAndRejectsNull()
    {
        AsyncEventHandler<object> first = _ => Task.CompletedTask;
        AsyncEventHandler<object> second = _ => Task.CompletedTask;
        var handler = first;

        var result = handler.GetInvocationList<AsyncEventHandler<object>>();
        handler += second;

        Assert.Collection(result, item => Assert.Same(first, item));

        AsyncEventHandler<object> nullHandler = null!;
        Assert.Throws<ArgumentNullException>(() => nullHandler.GetInvocationList<AsyncEventHandler<object>>());
    }
}
