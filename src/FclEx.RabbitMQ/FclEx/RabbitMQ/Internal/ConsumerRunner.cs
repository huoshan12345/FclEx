using System.Runtime.ExceptionServices;
using RabbitMQ.Client.Exceptions;

namespace FclEx.RabbitMQ.Internal;

internal sealed class ConsumerRunner<T> : IAsyncDisposable
{
    private readonly RabbitMQClient _client;
    private readonly IChannel _channel;
    private readonly RabbitMQConsumerOptions _options;
    private readonly Func<ReceivedMessage<T>, CancellationToken, Task<MessageHandlingResult>> _handler;
    private readonly ILogger _logger;
    private readonly AsyncEventingBasicConsumer _consumer;
    private readonly SemaphoreSlim _settlements = new(1, 1);
    private readonly CancellationTokenSource _dispose = new();
    private CancellationTokenSource _epoch = new();
    private readonly object _state = new();
    private readonly TaskCompletionSource<Exception?> _terminal = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private TaskCompletionSource<bool> _drained = CompletedDrain();
    private CancellationToken _handlerToken;
    private Task? _runTask;
    private Task? _disposeTask;
    private int _inFlight;
    private int _generation;
    private bool _started;
    private bool _stopping;
    private bool _disposing;

    internal string QueueName => _options.Queue.Name;

    internal ConsumerRunner(RabbitMQClient client, IChannel channel, RabbitMQConsumerOptions options,
        Func<ReceivedMessage<T>, CancellationToken, Task<MessageHandlingResult>> handler)
    {
        _client = client;
        _channel = channel;
        _options = options;
        _handler = handler;
        _logger = client.LoggerFactory.CreateLogger<ConsumerRunner<T>>();
        _consumer = new(channel);
        _consumer.ReceivedAsync += OnReceivedAsync;
        _consumer.UnregisteredAsync += OnUnregisteredAsync;
        _channel.ChannelShutdownAsync += OnShutdownAsync;
        _channel.CallbackExceptionAsync += OnCallbackExceptionAsync;
    }

    internal Task RunAsync(CancellationToken token)
    {
        lock (_state)
        {
            if (_disposing) throw new ObjectDisposedException(nameof(RabbitMQConsumer<T>));
            _client.ThrowIfDisposed();
            if (_started) throw new InvalidOperationException("A subscription can only run once.");
            _started = true;
            return _runTask = RunCoreAsync(token);
        }
    }

    private async Task RunCoreAsync(CancellationToken token)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, _dispose.Token);
        _handlerToken = linked.Token;
        Exception? failure = null;
        using var registration = linked.Token.Register(() => _terminal.TrySetResult(null));
        try
        {
            linked.Token.ThrowIfCancellationRequested();
            await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: _consumer,
                cancellationToken: linked.Token).ConfigureAwait(false);
            failure = await _terminal.Task.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (linked.IsCancellationRequested) { }
        finally
        {
            await StopCoreAsync().ConfigureAwait(false);
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }

    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        int generation;
        CancellationTokenSource invocation;
        lock (_state)
        {
            if (_stopping || _terminal.Task.IsCompleted || _handlerToken.IsCancellationRequested) return;
            generation = _generation;
            invocation = CancellationTokenSource.CreateLinkedTokenSource(_handlerToken, _epoch.Token);
            if (_inFlight++ == 0) _drained = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        using (invocation)
        try
        {
            ReceivedMessage<T> message;
            try
            {
                // A custom serializer may retain its input. Own the memory independently of the native callback.
                var body = (T)_client.Serializer.Deserialize(args.Body.ToArray(), typeof(T))!;
                message = new(body, args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid message {MessageId} in {Queue}; sending to dead-letter topology", args.BasicProperties.MessageId, QueueName);
                await SettleAsync(args.DeliveryTag, MessageHandlingResult.DeadLetter, generation).ConfigureAwait(false);
                return;
            }

            MessageHandlingResult result;
            try
            {
                result = await _handler(message, invocation.Token).ConfigureAwait(false);
                if (result is not (MessageHandlingResult.Completed or MessageHandlingResult.Retry or MessageHandlingResult.DeadLetter))
                    throw new InvalidOperationException($"Undefined handler disposition: {(int)result}.");
            }
            catch (OperationCanceledException) when (invocation.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Handler failed for {MessageId} in {Queue} at delivery count {DeliveryCount}", message.MessageId, QueueName, message.DeliveryCount);
                result = MessageHandlingResult.Retry;
            }
            await SettleAsync(args.DeliveryTag, result, generation).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Do not throw into the dispatcher and then continue with more deliveries after failed settlement.
            _terminal.TrySetResult(ex);
        }
        finally
        {
            lock (_state) if (--_inFlight == 0) _drained.TrySetResult(true);
        }
    }

    private async Task SettleAsync(ulong tag, MessageHandlingResult result, int generation)
    {
        using var deadline = new CancellationTokenSource(_options.ShutdownTimeout);
        await _settlements.WaitAsync(deadline.Token).ConfigureAwait(false);
        try
        {
            if (generation != Volatile.Read(ref _generation)) return;
            if (result == MessageHandlingResult.Completed)
                await _channel.BasicAckAsync(tag, multiple: false, cancellationToken: deadline.Token).ConfigureAwait(false);
            else
                await _channel.BasicRejectAsync(tag, requeue: result == MessageHandlingResult.Retry && _options.Queue.Retry.Enabled,
                    cancellationToken: deadline.Token).ConfigureAwait(false);
        }
        finally { _settlements.Release(); }
    }

    private Task OnUnregisteredAsync(object sender, ConsumerEventArgs args)
    {
        lock (_state)
            if (!_stopping) _terminal.TrySetResult(new InvalidOperationException($"The broker canceled subscription '{QueueName}'."));
        return Task.CompletedTask;
    }

    private Task OnCallbackExceptionAsync(object sender, CallbackExceptionEventArgs args)
    {
        _terminal.TrySetResult(args.Exception);
        return Task.CompletedTask;
    }

    private Task OnShutdownAsync(object sender, ShutdownEventArgs args)
    {
        CancellationTokenSource previous;
        lock (_state)
        {
            _generation++;
            previous = _epoch;
            _epoch = new();
            if (!_stopping && (!_client.AutomaticRecoveryEnabled || _client.Connection.IsOpen || args.Initiator == ShutdownInitiator.Application))
                _terminal.TrySetResult(new AlreadyClosedException(args));
        }
        try { previous.Cancel(); }
        finally { previous.Dispose(); }
        return Task.CompletedTask;
    }

    private async Task StopCoreAsync()
    {
        Task drain;
        lock (_state)
        {
            _stopping = true;
            drain = _drained.Task;
        }
        using var deadline = new CancellationTokenSource(_options.ShutdownTimeout);
        try
        {
            // Cancellation is advisory. A handler that nevertheless finishes successfully can still be acknowledged.
            _dispose.Cancel();
            if (_channel.IsOpen)
                foreach (var tag in _consumer.ConsumerTags.ToArray())
                    await _channel.BasicCancelAsync(tag, cancellationToken: deadline.Token).ConfigureAwait(false);
            await AsyncTools.WaitAsync(drain, deadline.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Subscription {Queue} did not drain cleanly; closing its channel", QueueName);
        }
        finally
        {
            Interlocked.Increment(ref _generation);
            // Native disposal can wait for noncooperative callbacks. Start cleanup, but bound the public shutdown wait.
            var closing = AsyncTools.CloseChannelAsync(_channel);
            try { await AsyncTools.WaitAsync(closing, deadline.Token).ConfigureAwait(false); }
            catch (Exception ex) { _logger.LogWarning(ex, "Channel cleanup for {Queue} exceeded shutdown bounds or failed", QueueName); }
            _channel.ChannelShutdownAsync -= OnShutdownAsync;
            _channel.CallbackExceptionAsync -= OnCallbackExceptionAsync;
            _consumer.ReceivedAsync -= OnReceivedAsync;
            _consumer.UnregisteredAsync -= OnUnregisteredAsync;
        }
    }

    public ValueTask DisposeAsync()
    {
        lock (_state)
        {
            _disposing = true;
            return new(_disposeTask ??= DisposeCoreAsync());
        }
    }

    private async Task DisposeCoreAsync()
    {
        _dispose.Cancel();
        if (_runTask is null)
            await StopCoreAsync().ConfigureAwait(false);
        else
            try { await _runTask.ConfigureAwait(false); }
            catch (Exception ex) { _logger.LogDebug(ex, "Subscription failure already reported by RunAsync for {Queue}", QueueName); }
        _dispose.Dispose();
        _epoch.Dispose();
    }

    private static TaskCompletionSource<bool> CompletedDrain()
    {
        var source = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        source.SetResult(true);
        return source;
    }
}
