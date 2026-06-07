namespace FclEx.Actions;

public partial class ExtensionsTests
{
    [Fact]
    public async Task ToUnit_MapsSuccessValueToUnit()
    {
        var (success, value, _, elapsed) = await SuccessAction.Create("value", TimeSpan.FromSeconds(1))
            .ToUnit()
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(Unit.Default, value);
        Assert.Equal(TimeSpan.FromSeconds(1), elapsed);
    }

    [Fact]
    public async Task MapValue_WhenPreviousFails_DoesNotInvokeMapper()
    {
        var invoked = false;

        var (success, _, ex, elapsed) = await ErrorAction.Create<int>("error", TimeSpan.FromSeconds(2))
            .MapValue(_ =>
            {
                invoked = true;
                return "value";
            })
            .ExecuteAsync();

        Assert.False(success);
        Assert.False(invoked);
        Assert.Equal("error", ex?.Message);
        Assert.Equal(TimeSpan.FromSeconds(2), elapsed);
    }

    [Fact]
    public async Task MapResult_WhenMapperReturnsError_ReturnsMappedError()
    {
        var (success, _, ex, _) = await SuccessAction.Create(1)
            .MapResult(_ => Operation.Error<string>("mapped"))
            .ExecuteAsync();

        Assert.False(success);
        Assert.Equal("mapped", ex?.Message);
    }

    [Fact]
    public async Task MapError_WhenPreviousSucceeds_DoesNotInvokeMapper()
    {
        var invoked = false;

        var (success, value, _, elapsed) = await SuccessAction.Create(3, TimeSpan.FromSeconds(2))
            .MapError(ex =>
            {
                invoked = true;
                return ex;
            })
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(3, value);
        Assert.False(invoked);
        Assert.Equal(TimeSpan.FromSeconds(2), elapsed);
    }

    [Fact]
    public async Task MapError_WhenPreviousFails_ReplacesException()
    {
        var replacement = new InvalidOperationException("replacement");

        var (success, _, ex, _) = await ErrorAction.Create<int>("original")
            .MapError(_ => replacement)
            .ExecuteAsync();

        Assert.False(success);
        Assert.Same(replacement, ex);
    }

    [Fact]
    public async Task MapErrorMessage_WhenPreviousFails_ChangesExceptionMessage()
    {
        var (success, _, ex, _) = await ErrorAction.Create<int>("original")
            .MapErrorMessage(message => $"mapped {message}")
            .ExecuteAsync();

        Assert.False(success);
        Assert.Equal("mapped original", ex?.Message);
    }

    [Fact]
    public void MapError_RejectsNullMapper()
    {
        var action = SuccessAction.Create(1);

        Assert.Throws<ArgumentNullException>(() => { _ = action.MapError(null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = action.MapErrorMessage(null!); });
    }

    [Fact]
    public async Task Reject_AfterSuccess_ReturnsErrorFromValue()
    {
        var (success, _, ex, _) = await SuccessAction.Create(9)
            .Reject(v => new InvalidOperationException($"bad {v}"))
            .ExecuteAsync();

        Assert.False(success);
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal("bad 9", ex?.Message);
    }

    [Fact]
    public async Task Reject_WhenPreviousFails_DoesNotInvokeErrorFactory()
    {
        var invoked = false;

        var (success, _, ex, _) = await ErrorAction.Create<int>("original")
            .Reject(_ =>
            {
                invoked = true;
                return "new";
            })
            .ExecuteAsync();

        Assert.False(success);
        Assert.False(invoked);
        Assert.Equal("original", ex?.Message);
    }

    [Fact]
    public async Task RejectIf_WhenConditionIsFalse_KeepsOriginalValue()
    {
        var (success, value, _, _) = await SuccessAction.Create(3)
            .RejectIf(v => v > 10, v => $"bad {v}")
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(3, value);
    }

    [Fact]
    public async Task RejectIf_WhenConditionIsTrue_ReturnsError()
    {
        var (success, _, ex, _) = await SuccessAction.Create(11)
            .RejectIf(v => v > 10, v => $"bad {v}")
            .ExecuteAsync();

        Assert.False(success);
        Assert.Equal("bad 11", ex?.Message);
    }

    [Fact]
    public async Task OnResult_InvokesCallbackForSuccessAndPreservesResult()
    {
        OperationResult<int> observed = default;

        var (success, value, _, elapsed) = await SuccessAction.Create(5, TimeSpan.FromSeconds(2))
            .OnResult(r => observed = r)
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(5, value);
        Assert.True(elapsed >= TimeSpan.FromSeconds(2), elapsed.ToString());
        Assert.True(elapsed < TimeSpan.FromSeconds(3), elapsed.ToString());
        Assert.True(observed.IsSuccess);
        Assert.Equal(5, observed.Value);
    }

    [Fact]
    public async Task OnResult_WhenCallbackThrows_ReturnsCallbackError()
    {
        var (success, _, ex, elapsed) = await SuccessAction.Create(5, TimeSpan.FromSeconds(2))
            .OnResult(_ => throw new InvalidOperationException("callback"))
            .ExecuteAsync();

        Assert.False(success);
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal("callback", ex.Message);
        Assert.True(elapsed >= TimeSpan.FromSeconds(2), elapsed.ToString());
        Assert.True(elapsed < TimeSpan.FromSeconds(3), elapsed.ToString());
    }

    [Fact]
    public async Task WhenResult_InvokesCallbackOnlyWhenConditionMatches()
    {
        var called = 0;

        var (success, value, _, _) = await SuccessAction.Create(5)
            .WhenResult(r => r.Value == 5, _ => called++)
            .WhenResult(r => r.Value == 6, _ => called++)
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(5, value);
        Assert.Equal(1, called);
    }

    [Fact]
    public async Task When_InvokesCallbackOnlyForSuccessfulMatchingValue()
    {
        var values = new List<int>();

        var (success, value, _, _) = await SuccessAction.Create(5)
            .When(v => v > 3, v => values.Add(v))
            .When(v => v > 10, v => values.Add(v))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(5, value);
        Assert.Equal(new[] { 5 }, values);
    }

    [Fact]
    public async Task OnFailed_AndOnException_InvokeOnlyForError()
    {
        OperationResult<int> observedResult = default;
        Exception? observedException = null;

        var (success, _, ex, _) = await ErrorAction.Create<int>("error")
            .OnFailed(r => observedResult = r)
            .OnException(e => observedException = e)
            .ExecuteAsync();

        Assert.False(success);
        Assert.Equal("error", ex?.Message);
        Assert.True(observedResult.IsError);
        Assert.Same(ex, observedResult.Exception);
        Assert.Same(ex, observedException);
    }

    [Fact]
    public async Task OnValue_DoesNotInvokeCallbackForError()
    {
        var invoked = false;

        var (success, _, ex, _) = await ErrorAction.Create<int>("error")
            .OnValue(_ => invoked = true)
            .ExecuteAsync();

        Assert.False(success);
        Assert.False(invoked);
        Assert.Equal("error", ex?.Message);
    }

    [Fact]
    public async Task RetryOnceIf_WhenConditionMatches_ReexecutesActionOnce()
    {
        var count = 0;
        var action = Operation.Action<int>(_ => ++count);

        var (success, value, _, _) = await action
            .RetryOnceIf(v => v == 1)
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(2, value);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task RetryOnceIf_WhenConditionDoesNotMatch_ReturnsFirstResult()
    {
        var count = 0;
        var action = Operation.Action<int>(_ => ++count);

        var (success, value, _, _) = await action
            .RetryOnceIf(v => v > 1)
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(1, value);
        Assert.Equal(1, count);
    }

    [Fact]
    public void RetryOnceIf_RejectsNullCondition()
    {
        Assert.Throws<ArgumentNullException>(() => SuccessAction.Create(1).RetryOnceIf(null!));
    }

    [Fact]
    public async Task RepeatUntil_StopsWhenUntilConditionMatches()
    {
        var count = 0;
        var action = Operation.Action<int>(_ => ++count);

        var (success, value, _, _) = await action
            .RepeatUntil(v => v == 3, 0)
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(3, value);
        Assert.Equal(3, count);
    }

    [Fact]
    public void RepeatUntil_RejectsNullCondition()
    {
        Assert.Throws<ArgumentNullException>(() => SuccessAction.Create(1).RepeatUntil(null!, 0));
    }

    [Fact]
    public async Task Chain_RunsActionsInOrder()
    {
        var order = new List<int>();
        IAction<int>[] actions =
        [
            Operation.Action<int>(_ =>
            {
                order.Add(1);
                return 1;
            }),
            Operation.Action<int>(_ =>
            {
                order.Add(2);
                return 2;
            })
        ];

        var (success, value, _, _) = await actions.Chain().ExecuteAsync();

        Assert.True(success);
        Assert.Equal(2, value);
        Assert.Equal(new[] { 1, 2 }, order);
    }

    [Fact]
    public async Task CombineInSeries_ReturnsValuesInOrder()
    {
        var (success, value, _, _) = await new[]
        {
            SuccessAction.Create(1),
            SuccessAction.Create(2),
        }.CombineInSeries().ExecuteAsync();

        Assert.True(success);
        Assert.Equal(new[] { 1, 2 }, value);
    }

    [Fact]
    public async Task CombineInParallel_ReturnsValuesInOrder()
    {
        var (success, value, _, _) = await new[]
        {
            SuccessAction.Create(1),
            SuccessAction.Create(2),
        }.CombineInParallel().ExecuteAsync();

        Assert.True(success);
        Assert.Equal(new[] { 1, 2 }, value);
    }

    [Fact]
    public async Task Chain_WithEmptySequence_ReturnsDefaultValue()
    {
        var (success, value, _, _) = await Array.Empty<IAction<int>>()
            .Chain()
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(0, value);
    }

    [Fact]
    public async Task ExecuteAsync_WithRetry_RetriesUntilSuccess()
    {
        var attempts = 0;
        var sleepIndexes = new List<int>();
        var action = Operation.Action<int>(_ =>
        {
            attempts++;
            return attempts < 3
                ? Operation.Error<int>($"error {attempts}")
                : Operation.Success(42);
        });

        var (success, value, _, _) = await action.ExecuteAsync(
            retryCount: 3,
            retryCondition: r => r.Exception?.Message != "stop",
            sleepDurationProvider: i =>
            {
                sleepIndexes.Add(i);
                return TimeSpan.Zero;
            });

        Assert.True(success);
        Assert.Equal(42, value);
        Assert.Equal(3, attempts);
        Assert.Equal(new[] { 1, 2 }, sleepIndexes);
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryConditionFalse_StopsAfterFirstFailure()
    {
        var attempts = 0;
        var action = Operation.Action<int>(_ =>
        {
            attempts++;
            return Operation.Error<int>("stop");
        });

        var (success, _, ex, _) = await action.ExecuteAsync(
            retryCount: 5,
            retryCondition: _ => false);

        Assert.False(success);
        Assert.Equal("stop", ex?.Message);
        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task RunAsync_DropsValueButPreservesSuccess()
    {
        var (success, ex, _) = await SuccessAction.Create(5).RunAsync();

        Assert.True(success);
        Assert.Null(ex);
    }
}
