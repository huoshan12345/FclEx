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
