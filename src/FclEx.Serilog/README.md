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
- Default-constructed JSON formatters own independent options, including nested
  exception options. Passing the same options explicitly shares configuration;
  finish configuring it before concurrent use.
- Nonempty basic JSON field names must be distinct. Event properties and flattened
  New Relic linking metadata cannot override them: colliding properties are omitted
  after the naming policy and `@` escaping are applied. A configured exception field
  remains reserved even for events without exceptions.
- `JsonFormatterOptions.PropertyCollisionPolicy` controls other top-level property
  collisions after naming and escaping: `KeepFirst` (default), `KeepLast`, or `Throw`.
  Event properties and flattened linking metadata are considered in enumeration
  order. `KeepLast` replaces the value at the first occurrence's output position.
  `Throw` raises `InvalidOperationException` without appending a partial event.
  Nested value formatting is unaffected, and basic field names remain reserved.
- Destructured scope property names use a thread-safe LRU cache of at most 1024
  entries, allowing old dynamic names to be collected after eviction.
- `LogstashSinkOptions.TcpSendTimeout` defaults to 30 seconds and covers connecting
  and sending one serialized TCP batch. On expiry the connection closes and a
  `TimeoutException` reaches batching for retry. Formatting time is excluded; UDP
  is unaffected. Retries can duplicate events from a partially sent batch.
- `ExceptionFormatOptions.IndexPolicy` selects when exception indexes are included:
  `None`, `Always`, `WhenMultiple`, or `WhenBranched`. The default is `WhenBranched`.
- `MaxMessageLength` limits retained message content; an appended `...` is outside
  that limit. Duplicate-message omission is checked against the full message first.
- `MessageExcluder` searches raw template text, while `PropertyExcluder` checks a
  top-level property's presence or a string scalar substring. Neither searches
  recursively through structured values.
- `LogEnricher` fills missing properties and preserves values already on an event.
  Its static host metadata is shared across instances and should be configured
  before concurrent logging begins.
- `SetSelfLog` changes the global diagnostic destination. Disposing its handle
  disables diagnostics, keeps the writer open, and does not restore a previous
  destination. Use a synchronized writer for concurrent diagnostic output.

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
6. Replace `ExceptionIndexOptions` and `ExceptionFormatOptions.IndexOptions` with
   `ExceptionIndexPolicy` and `IndexPolicy`. These are mutually exclusive policies:
   old `None` becomes `None`, `Include` becomes `Always`, `Include | IncludeForMultiple`
   becomes `WhenMultiple`, and combinations containing `IncludeForMultiBranched`
   together with `Include` (including old `Default`) become `WhenBranched`.
   The old conditional flags alone never enabled indexes; choose the corresponding
   new policy to enable the intended condition directly.
7. `WriteTo.Wrap` and `WrapSinkPipeline` pass an owned aggregate sink to the wrapper
   factory and register the returned wrapper directly. Custom wrappers must forward
   disposal to that aggregate, implement asynchronous disposal when needed, and
   forward failure listeners for fallback support. `LogEventMutateSink` already
   fulfills these responsibilities.
8. Formatter subclasses should override `GetPropertyCandidates` to customize
   event/linking-metadata properties before naming and conflict resolution.
   This replaces the `WriteLinkingMetadataProperties` writing hook.
   `WriteFormattedJsonData` now receives the final JSON name and must not apply
   the naming policy again.
9. `FormatExceptionSink` and `WriteTo.FormatException(...)` have been removed.
   Configure all sinks, then use `WrapSinkPipeline` with `LogEventMutateSink` to
   normalize and format exceptions once before dispatching to the whole group.
   `SerilogConfiguration.CreateLogger()` already adds this stage automatically.

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

`LogEventMutateSink` owns its directly supplied inner sink. `WrapSinkPipeline`
transfers all current `WriteTo` sinks into one aggregate and passes ownership to
the wrapper factory. The ownership chain is wrapper -> aggregate -> actual sinks.
Each layer releases its directly owned resources and forwards failure notifications.
`SerilogLoggerProviderWithoutExternalScope` forwards asynchronous disposal on supported
targets; its `dispose` constructor argument determines whether it owns the destination
logger. Use `dispose: false` when another owner, such as the root container, disposes it.

With an owning wrapper such as `LogEventMutateSink`, `WriteTo.Wrap` and
`WrapSinkPipeline` preserve failure notifications for fallback chains, including
groups of multiple sinks. The aggregate releases its sinks once and attempts all
registered sink cleanups even if one fails. Cleanup failures propagate, with multiple
failures aggregated. A custom wrapper is responsible for releasing its aggregate
even when cleanup of the wrapper's own resources fails.

Call `WrapSinkPipeline` after registering all sinks and before `CreateLogger`.
It wraps the current `WriteTo` group once per event. `AuditTo` sinks and later sink
registrations are outside that group. Global filters run before the wrapper, and
per-sink level restrictions run after it. `WrapAllSinks` remains the separate API
for wrapping each currently registered sink individually.

For example, normalize and format exceptions for every configured `WriteTo` sink:

```csharp
using var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WrapSinkPipeline(inner => new LogEventMutateSink(inner, e => e.FormatException()))
    .CreateLogger();
```

This generic wrapping helper does not add exception-level filtering. Use
`SerilogConfiguration` for the minimum-level recheck described below.

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
