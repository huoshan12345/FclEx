namespace FclEx.RabbitMQ;

/// <summary>Specifies the disposition of one handled delivery. Zero and undefined values are errors, never success.</summary>
public enum MessageHandlingResult
{
    /// <summary>Acknowledge only after the handler's work has completed.</summary>
    Completed = 1,
    /// <summary>Reject with requeue so the original quorum queue applies its retry policy; dead-letter when retry is disabled.</summary>
    Retry = 2,
    /// <summary>Reject without requeue so the original message is transferred to its dead-letter queue.</summary>
    DeadLetter = 3,
}
