namespace FclEx.Kafka;

public enum KafkaErrorType
{
    PollError,
    ConsumeError,
    CommitError,
    FromErrorHandler,
}