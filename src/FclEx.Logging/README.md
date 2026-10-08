# FclEx.Logging

`Microsoft.Extensions.Logging` helpers for FclEx.

## What Is Included

- Logger factory and provider convenience helpers.
- In-memory collectors for log messages, metadata, and scopes.
- Scoped logging-property helpers.
- Logging helpers for supplied operation durations.
- Null logger fallbacks and cleanup utilities.
- Logging provider registration and service cleanup.

## Usage Notes

- This package uses Microsoft logging abstractions.
- Serilog-specific helpers live in `FclEx.Serilog`.
- Use scoped properties when downstream providers need structured context for an operation.

## Collecting Logs

For a standalone collector:

```csharp
using FclEx.Logging;
using Microsoft.Extensions.Logging;

var logger = new CollectingLogger("example");
using (logger.PushProperty("RequestId", 42))
{
    logger.LogInformation("Processing {Count} items", 3);
}

var entry = logger.Entries[0];
// entry.Message: "Processing 3 items"
// entry.State: the original structured message state
// entry.Scopes: the active scopes, ordered from outermost to innermost
logger.Clear();
```

Register the collector through `ILoggingBuilder` when using dependency injection:

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging(builder => builder
    .SetMinimumLevel(LogLevel.Trace)
    .AddCollecting());

using var serviceProvider = services.BuildServiceProvider();
var factory = serviceProvider.GetRequiredService<ILoggerFactory>();
var collector = serviceProvider.GetRequiredService<CollectingLoggerProvider>();
factory.CreateLogger("example").LogDebug("Collected message");

var entries = collector.Entries;
collector.Clear();
```

For an existing factory, `factory.AddCollecting()` returns the newly added collector.
Repeated calls to the builder extension register one singleton collector; repeated calls
to the factory extension add independent collectors. Factory filters apply in both cases.

`Entries` returns a synchronized snapshot. Later writes and `Clear()` do not alter earlier
snapshots. A provider combines entries from every category, and its loggers share that store.
Scope stack membership is captured when a message is logged, while state and scope values
remain the original objects and are not deep copied. Standalone collectors maintain their
own async scope context; provider-created loggers share the factory's scope context.

Collectors accept every defined level except `LogLevel.None`. They retain entries until
explicitly cleared, including after provider disposal, so clear them between test phases
or when the retained data is no longer needed.

## Property Wrappers and Scopes

`logger.With("UserId", userId)` creates a `PropertiesLogger`. Property definitions are
copied when the wrapper is constructed; object values are retained by reference.
`LazyLoggerProperty.ValueFactory` runs once per enabled log call. Its result is captured
before the property scope begins, so all providers and later inspections see the same
value for that entry. Disabled calls do not evaluate lazy factories. Factory exceptions
propagate without writing the message, and temporary scopes are restored on failure.

`logger.Properties()` returns a `LoggerPropertyScope`. Each `Push` immediately starts
a property scope; disposing the group releases its scopes in reverse order. Dispose
nested scopes before their enclosing scopes. `PushProperty` enumerates an input sequence
once and captures its property definitions; null or empty sequences start no scope.
Structured property output requires a provider that supports structured scopes.
Serilog's `destructureObjects` overloads live in `FclEx.Serilog`.

`RemoveLogging` removes factories, providers, untyped loggers, and both open and closed
`ILogger<T>` registrations. It unregisters logging rather than replacing it with null
loggers, and leaves options, concrete provider registrations, and existing service
providers unchanged. `AddLogging` can register logging again afterward.

## API Changes

- The factory extension `SetMinimumLevel` and its private-field reflection helper have
  been removed. Configure initial levels through `ILoggingBuilder.SetMinimumLevel`;
  use Microsoft's options/configuration change notifications for runtime updates.
- `LoggerProperties` is now `LoggerPropertyScope`, and its integration extension classes
  are named `LoggerPropertyScopeExtensions`.
- `FclEx.Logging.Extensions` is now `FclEx.Logging.LoggerExtensions`.
- `LazyLoggerProperty.Value` is now `ValueFactory`.
- Provider type categories now match Microsoft's factory categories, including the
  exclusion of generic arguments. Update category-specific filters if necessary.
