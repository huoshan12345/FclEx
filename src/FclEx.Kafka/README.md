# FclEx.Kafka

Kafka consumer, serialization, header, and logging helpers built on Confluent.Kafka.

## What Is Included

- Sequential consumers that commit offsets only after successful handling.
- Consumer configuration and message-handler options.
- JSON value deserializers and UTF-8 header helpers.
- Structured logging for Kafka errors and delivery results.

## Usage Notes

- Reference the `FclEx.Kafka` package and import `FclEx.Kafka` and `Confluent.Kafka`.
- Connection settings use Confluent's `ConsumerConfig` directly.
- Integration tests require a reachable Kafka broker.

## Kafka Consumer

Start the consumer from a `ConsumerConfig` and await the returned task for its entire lifetime:

```csharp
var config = new ConsumerConfig
{
    BootstrapServers = "localhost:9092",
    GroupId = "orders",
    AutoOffsetReset = AutoOffsetReset.Earliest,
};

await config.StartKafkaConsumer(new KafkaConsumerOptions<string>
{
    Name = "order-handler",
    Topic = "orders",
    Deserializer = Deserializers.Utf8,
    MessageHandler = HandleOrderAsync,
    CancellationToken = stoppingToken,
});
```

The consumer copies the configuration and disables automatic commits and automatic offset storage.
It handles records sequentially and commits each offset only after its handler succeeds, including tombstones.
A handler, deserialization, polling, or commit failure stops the consumer. Later records cannot commit past a failed record.
`ErrorHandler` reports message-handler failures; completing it does not authorize committing the failed record.
If that callback also fails, the returned task retains the original handler exception.

Cancellation interrupts polling. In-flight handlers must observe the token themselves: successful work is still committed,
while a handler that throws `OperationCanceledException` after cancellation leaves its record uncommitted.
The consumer closes and disposes on exit. Resume with the same group to replay uncommitted records, subject to broker retention
and the configured offset-reset policy; use `Earliest` if a group with no committed offset must replay existing records.
Handlers must tolerate duplicate delivery when processing succeeds but committing its offset fails.

The previous `ConsumerBuilder<string, T>.StartKafkaConsumer` entry point has been replaced by
`ConsumerConfig.StartKafkaConsumer`, so this helper can own and enforce its offset-commit configuration.

## Kafka Integration Tests

Tests use the shared `CoreTestsFixture` configuration. Set the `Kafka` section in
`test/FclEx.Core.Tests/appsettings.json`, with credentials in the ignored `appsettings.decrypted.json`
or `appsettings.local.json` beside it. The shared test project copies these files to the test output directory.
Property names match Confluent's `ClientConfig`, including `BootstrapServers`, `SecurityProtocol`,
`SaslMechanism`, `SaslUsername`, and `SaslPassword`. These integration tests require connection settings;
missing configuration does not skip them.

The test user needs permission to create/delete topics and consumer groups, produce/consume messages, and query committed offsets.
Tests use unique topics and group IDs and remove them after each case.
When a broker advertises `localhost:9092`, forward local port 9092 to the broker through SSH before running the tests.
For an IPv4-only tunnel, set `Kafka:BrokerAddressFamily` to `V4` so advertised `localhost` addresses resolve consistently.

```powershell
dotnet test test/FclEx.Kafka.Tests/FclEx.Kafka.Tests.csproj -c Release -f net10.0 --filter "FullyQualifiedName~FclEx.Kafka.ConsumerConfigExtensionsTests"
```
