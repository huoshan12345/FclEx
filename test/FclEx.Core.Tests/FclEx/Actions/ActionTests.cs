namespace FclEx.Actions;

public class ActionTests
{
    [Fact]
    public async Task SuccessAction_ReturnsConfiguredValueAndElapsed()
    {
        var (success, value, _, elapsed) = await new SuccessAction<string>(
                "value",
                TimeSpan.FromSeconds(1))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal("value", value);
        Assert.Equal(TimeSpan.FromSeconds(1), elapsed);
    }

    [Fact]
    public void SuccessAction_RejectsNullValue()
    {
        Assert.Throws<ArgumentNullException>(() => new SuccessAction<string?>(null!));
    }

    [Fact]
    public async Task ErrorAction_FromString_ReturnsSimpleExceptionAndElapsed()
    {
        var (success, _, ex, elapsed) = await new ErrorAction<int>(
                "error",
                TimeSpan.FromSeconds(2))
            .ExecuteAsync();

        Assert.False(success);
        Assert.IsType<SimpleException>(ex);
        Assert.Equal("error", ex?.Message);
        Assert.Equal(TimeSpan.FromSeconds(2), elapsed);
    }

    [Fact]
    public async Task ErrorAction_FromException_ReturnsSameException()
    {
        var exception = new InvalidOperationException("error");

        var (success, _, ex, _) = await new ErrorAction<int>(exception).ExecuteAsync();

        Assert.False(success);
        Assert.Same(exception, ex);
    }

    [Fact]
    public async Task ResultAction_ReturnsStoredResult()
    {
        OperationResult<int> stored = (10, TimeSpan.FromSeconds(3));

        var (success, value, _, elapsed) = await ResultAction.Create(stored).ExecuteAsync();

        Assert.True(success);
        Assert.Equal(10, value);
        Assert.Equal(TimeSpan.FromSeconds(3), elapsed);
    }

    [Fact]
    public async Task OperationAction_PassesCancellationTokenToDelegate()
    {
        using var source = new CancellationTokenSource();
        var action = new OperationAction<int>(token => Operation.Success(token == source.Token ? 1 : 0));

        var (success, value, _, _) = await action.ExecuteAsync(source.Token);

        Assert.True(success);
        Assert.Equal(1, value);
    }

    [Fact]
    public void OperationAction_ConstructorRejectsNullDelegate()
    {
        Assert.Throws<ArgumentNullException>(() => new OperationAction<int>(null!));
    }

    [Fact]
    public void MapValueAction_ConstructorRejectsNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() => new MapValueAction<int, string>(null!, _ => ""));
        Assert.Throws<ArgumentNullException>(() => new MapValueAction<int, string>(SuccessAction.Create(1), null!));
    }

    [Fact]
    public void MapResultAction_ConstructorRejectsNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() => new MapResultAction<int, string>(null!, _ => Operation.Success("")));
        Assert.Throws<ArgumentNullException>(() => new MapResultAction<int, string>(SuccessAction.Create(1), null!));
    }

    [Fact]
    public void ThenAction_ConstructorRejectsNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() => new ThenAction<int, string>(null!, _ => SuccessAction.Create("")));
        Assert.Throws<ArgumentNullException>(() => new ThenAction<int, string>(SuccessAction.Create(1), null!));
    }

    [Fact]
    public void ThenResultAction_ConstructorRejectsNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() => new ThenResultAction<int, string>(null!, _ => SuccessAction.Create("")));
        Assert.Throws<ArgumentNullException>(() => new ThenResultAction<int, string>(SuccessAction.Create(1), null!));
    }

    [Fact]
    public void ThenWithAction_ConstructorRejectsNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() => new ThenWithAction<int, string>(null!, _ => SuccessAction.Create("")));
        Assert.Throws<ArgumentNullException>(() => new ThenWithAction<int, string>(SuccessAction.Create(1), null!));
    }

    [Fact]
    public void SeriesAction_ConstructorRejectsNullActions()
    {
        Assert.Throws<ArgumentNullException>(() => new SeriesAction<int>(null!));
    }

    [Fact]
    public void ParallelAction_ConstructorRejectsNullActions()
    {
        Assert.Throws<ArgumentNullException>(() => new ParallelAction<int>(null!));
    }

    [Fact]
    public async Task SeriesAction_WithEmptySequence_ReturnsEmptyArray()
    {
        var (success, value, _, _) = await SeriesAction.Create(Array.Empty<IAction<int>>()).ExecuteAsync();

        Assert.True(success);
        Assert.NotNull(value);
        Assert.Empty(value);
    }

    [Fact]
    public async Task SeriesAction_StopsAfterFirstError()
    {
        var invoked = false;
        IAction<int>[] actions =
        [
            SuccessAction.Create(1),
            ErrorAction.Create<int>("error"),
            Operation.Action<int>(_ =>
            {
                invoked = true;
                return 3;
            })
        ];

        var (success, _, ex, _) = await SeriesAction.Create(actions).ExecuteAsync();

        Assert.False(success);
        Assert.False(invoked);
        Assert.Equal("error", ex?.Message);
    }

    [Fact]
    public async Task SeriesAction_ReturnsValuesInExecutionOrder()
    {
        var (success, value, _, _) = await SeriesAction.Create([
            SuccessAction.Create(1),
            SuccessAction.Create(2)
        ]).ExecuteAsync();

        Assert.True(success);
        Assert.Equal(new[] { 1, 2 }, value);
    }

    [Fact]
    public async Task ParallelAction_WithEmptySequence_ReturnsEmptyArray()
    {
        var (success, value, _, _) = await ParallelAction.Create(Array.Empty<IAction<int>>()).ExecuteAsync();

        Assert.True(success);
        Assert.NotNull(value);
        Assert.Empty(value);
    }

    [Fact]
    public async Task ParallelAction_StartsAllActionsBeforeReturningFirstErrorByInputOrder()
    {
        var invoked = new List<int>();
        IAction<int>[] actions =
        [
            new OperationAction<int>(_ =>
            {
                invoked.Add(1);
                return Operation.Error<int>("first");
            }),
            new OperationAction<int>(_ =>
            {
                invoked.Add(2);
                return Operation.Error<int>("second");
            })
        ];

        var (success, _, ex, _) = await ParallelAction.Create(actions).ExecuteAsync();

        Assert.False(success);
        Assert.Equal("first", ex?.Message);
        Assert.Equal(new[] { 1, 2 }, invoked);
    }

    [Fact]
    public async Task ParallelAction_ReturnsValuesInInputOrder()
    {
        var (success, value, _, _) = await ParallelAction.Create([
            SuccessAction.Create(1),
            SuccessAction.Create(2)
        ]).ExecuteAsync();

        Assert.True(success);
        Assert.Equal(new[] { 1, 2 }, value);
    }
}
