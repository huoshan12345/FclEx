namespace FclEx.RabbitMQ;

public class PublishMessageTests
{
    [Fact]
    public void Headers_AreDeepSnapshots_AndIdIsStable()
    {
        var bytes = new byte[] { 1, 2 };
        var nested = new Dictionary<string, object?> { ["bytes"] = bytes };
        var headers = new Dictionary<string, object?> { ["nested"] = nested };
        var message = new PublishMessage<string>("body", headers: headers);
        bytes[0] = 9;
        nested["other"] = "mutation";
        headers.Clear();
        var copy = Assert.IsType<Dictionary<string, object?>>(message.Headers["nested"]);
        Assert.Single(copy);
        Assert.Equal(new byte[] { 1, 2 }, Assert.IsType<byte[]>(copy["bytes"]));
        Assert.IsType<byte[]>(copy["bytes"])[0] = 8;
        var next = Assert.IsType<Dictionary<string, object?>>(message.Headers["nested"]);
        Assert.Equal(new byte[] { 1, 2 }, next["bytes"]);
        Assert.False(string.IsNullOrWhiteSpace(message.MessageId));
        Assert.Equal(message.MessageId, message.MessageId);
    }

    public static TheoryData<string> TransportHeaders => new()
    {
        "x-opt-delivery-time", "x-delay", "x-death", "x-delivery-count", "x-acquired-count", "d-error-times", "x-first-death-queue",
    };

    [Theory]
    [MemberData(nameof(TransportHeaders))]
    public void TransportHeaders_CannotBeForged(string name)
        => Assert.Throws<ArgumentException>(() => new PublishMessage<string>("body", headers: new Dictionary<string, object?> { [name] = 1L }));

    [Fact]
    public void InvalidFieldTables_DoNotRetainUnknownMutableObjectsOrCycles()
    {
        Assert.Throws<ArgumentException>(() => new PublishMessage<string>("body", headers: new Dictionary<string, object?> { ["value"] = new object() }));
        var cyclic = new Dictionary<string, object?>();
        cyclic["self"] = cyclic;
        Assert.Throws<ArgumentException>(() => new PublishMessage<string>("body", headers: cyclic));
    }

    [Fact]
    public void ScheduleValidation_DoesNotAllowNegativeOrAmbiguousDelays()
    {
        Assert.Throws<ArgumentException>(() => new PublishMessage<string>("body", new() { Delay = TimeSpan.FromTicks(-1) }));
        Assert.Throws<ArgumentException>(() => new PublishMessage<string>("body", new() { Delay = TimeSpan.Zero, DeliverAt = DateTimeOffset.UtcNow }));
    }

    [Fact]
    public void Scheduling_RoundsUp_AndDoesNotDelayPastOrZeroTimes()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        Assert.Equal(now.ToUnixTimeMilliseconds() + 1, RabbitMQPublisher<string>.DeliveryTime(new() { Delay = TimeSpan.FromTicks(1) }, now));
        Assert.Null(RabbitMQPublisher<string>.DeliveryTime(new() { Delay = TimeSpan.Zero }, now));
        Assert.Null(RabbitMQPublisher<string>.DeliveryTime(new() { DeliverAt = now.AddSeconds(-1) }, now));
        Assert.Throws<ArgumentOutOfRangeException>(() => RabbitMQPublisher<string>.DeliveryTime(new() { Delay = TimeSpan.MaxValue }, now));
    }
}
