namespace FclEx.Messaging;

public enum KafkaErrorType
{
    PollError,
    ConsumeError,
    CommitError,
    FromErrorHandler,
}