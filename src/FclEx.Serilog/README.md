# FclEx.Serilog

Serilog configuration and container-owned logging helpers for FclEx.

## What Is Included

- Serilog configuration helpers and lazy, container-owned service registration.
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
  actions in registration order. Configure them on the options before calling
  `AddSerilog`. They run during service registration, before deferred logger creation.
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
3. `CreateLogger()` returns `Serilog.Core.Logger`. The caller owns the returned
   logger and disposes it with `using`, or `await using` on supported targets.
4. Replace the mixed `ConfigureAction` delegate and two-parameter configuration
   callbacks with `Configure(Action<LoggerConfiguration>)`. Set integration options
   and register Microsoft logging builder actions before calling `AddSerilog`.
   `Configure` is now an instance method, so use `options.Configure(action)` for
   former static calls to `SerilogConfigurationExtensions.Configure`.
5. `AddSerilog` creates a separate singleton logger for each root service provider
   when logging is first resolved. Applications choose when to assign `Log.Logger`.

For example, prepare the configuration before registration.

```csharp
var options = new SerilogConfiguration()
    .Configure(logger => logger.MinimumLevel.Debug())
    .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));

services.AddSerilog(options);
```

## Logger Ownership

`CreateLogger()` creates a caller-owned logger. `AddSerilog()` registers a factory
whose logger is owned by the root service provider. Disposing a child scope preserves
the root logger. Asynchronous root-provider disposal awaits asynchronous sink cleanup
on supported targets.

Serilog configuration actions run once for each created root logger. Create
per-container disposable sinks inside those actions when building multiple providers
from the same service collection. Complete configuration before registration.

To use the container-owned logger globally, assign it explicitly after building
the application or root provider.

```csharp
Log.Logger = provider.GetRequiredService<Serilog.ILogger>();
```

The root provider retains ownership of that logger. If a configuration action fails,
registered sinks are released before the error is propagated. Failures in both
configuration and cleanup are reported together as an `AggregateException`.

## Exception Log Levels

Loggers created by `SerilogConfiguration.CreateLogger` use `LogException.Level` as
the event level before applying event filters and sink restrictions. The resulting
level must also pass the logger's current minimum level, including source overrides
and dynamic level switches. `LogLevel.None` suppresses the event.

After event filters, exception unwrapping and optional formatting run once before
events are dispatched to `WriteTo` sinks. All sinks receive the same normalized
exception and level; nested `LogException` levels are not reapplied for later sinks.

The original log call must first pass Serilog's initial minimum-level check and any
upstream filters. For example, an Information call carrying an Error `LogException`
is discarded when the logger's minimum is Error. The exception level cannot restore
an event rejected earlier. Choose an original call level that passes those checks.
Microsoft `ILoggerFactory` filters evaluate the original call level.

An Error call carrying a Warning `LogException` is recorded as Warning only when
the final level passes the Serilog minimum level, event filters, and sink restrictions.
It is discarded when the applicable Serilog minimum is Error.
