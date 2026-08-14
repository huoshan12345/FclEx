namespace FclEx.Utils;

/// <summary>
/// Describes what a consumer will do after a consumption failure.
/// </summary>
public enum ConsumerFailureAction
{
    /// <summary>The item or singleton segment will be retried.</summary>
    Retry,

    /// <summary>The failed batch or segment will be divided into smaller segments.</summary>
    Split,

    /// <summary>The item will be discarded because its retry limit has been reached.</summary>
    Discard,

    /// <summary>The work will be abandoned because the consumer is stopping.</summary>
    Abandon,
}

/// <summary>Describes a failed single-item consumption attempt.</summary>
public sealed record ItemConsumptionFailure<T>(
    T Item,
    Exception Exception,
    int AttemptNumber,
    ConsumerFailureAction Action);

/// <summary>Describes an item discarded by a single-item consumer.</summary>
public sealed record DiscardedItem<T>(
    T Item,
    Exception Exception,
    int AttemptCount);

/// <summary>Describes a failed batch or retry-segment consumption attempt.</summary>
public sealed record BatchConsumptionFailure<T>(
    IReadOnlyList<T> Items,
    Exception Exception,
    int SingletonRetryCount,
    ConsumerFailureAction Action);

/// <summary>Describes a singleton item discarded by a batch consumer.</summary>
public sealed record DiscardedBatchItem<T>(
    T Item,
    Exception Exception,
    int RetryCount);

/// <summary>Describes an exception thrown by a notification listener.</summary>
public sealed record ConsumerListenerFailure(
    string NotificationName,
    Exception Exception);
