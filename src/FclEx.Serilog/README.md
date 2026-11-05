# FclEx.Serilog

Serilog helpers for FclEx.

## What Is Included

- Serilog configuration helpers and service registration.
- Enrichers and structured-property helpers.
- Excluder/filter types for source, message, property, and exception filtering.
- JSON formatter and exception formatting options.
- Log event extensions and level helpers.
- Sinks for log mutation, exception formatting, Logstash, and New Relic.

## Usage Notes

- This package bridges FclEx HTTP/logging helpers with Serilog.
- Slack-specific Serilog sinks live in `FclEx.Serilog.Slack`.
- Use the formatter options to keep exception output consistent across sinks.
- The Microsoft logger property helpers return `LoggerPropertyScope`. Serilog's
  `destructureObjects` overloads prefix scope property names with `@` to request object
  destructuring; this convention is specific to the Serilog provider.
- `ConfigureLogging` and `ConfigureLoggingBuilder` append Microsoft logging builder
  actions in registration order. `AddSerilog` runs them after Serilog configuration
  actions have completed, including actions added inside its configuration callback.
- `ApplyMicrosoftLoggingFilter` reads provider-independent level rules from the
  `Logging` section. Category matching ignores case and supports one `*` wildcard.
  The longest matching category wins, with later rules winning ties. Configuration
  is read once when the filter is added.
- New Relic batches succeed on HTTP 202 Accepted. Serialization, transport, timeout,
  and other HTTP response failures propagate to Serilog's batching infrastructure
  for diagnostics and retries.
- New Relic sinks own their default HTTP client and release it during disposal.
  The `NewRelicSink(HttpClient, ...)` overload keeps the caller's client settings and
  uses caller ownership by default. Pass `disposeHttpClient: true` to transfer
  ownership to the sink. License keys are attached to individual requests.
- Logstash endpoints use `tcp://host:port` or `udp://host:port`, with an explicit
  destination port from 1 through 65535. The sink validates the URI during construction.
- Empty JSON formatter field names omit those fields. Exception formatting failures
  produce a diagnostic string after the incomplete exception fragment is discarded.

## API Migration

1. Replace calls to `MatchMaxLeveOrNull` with `MatchMaxLevelOrNull`.
2. The `LogstashInputType` enum has been removed. Select the transport using the
   `tcp` or `udp` scheme in `LogstashSinkOptions.Uri`.

## Exception Log Levels

Loggers created by `SerilogConfiguration.CreateLogger` use `LogException.Level` as
the event level before applying event filters and sink restrictions. The resulting
level must also pass the logger's current minimum level, including source overrides
and dynamic level switches. `LogLevel.None` suppresses the event.

The original log call must first pass Serilog's initial minimum-level check and any
upstream filters. For example, an Information call carrying an Error `LogException`
is discarded when the logger's minimum is Error. The exception level cannot restore
an event rejected earlier. Choose an original call level that passes those checks.
Microsoft `ILoggerFactory` filters evaluate the original call level.

An Error call carrying a Warning `LogException` is recorded as Warning only when
the final level passes the Serilog minimum level, event filters, and sink restrictions.
It is discarded when the applicable Serilog minimum is Error.
