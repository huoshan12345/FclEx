# FclEx.Xunit.v3

xUnit v3 helpers for FclEx test projects.

## What Is Included

- FclEx test helpers for xUnit v3, adapted to the current xUnit v3 APIs.
- Extended assertions, conditional attributes, output helpers, logging integration, and test utilities.
- Integrated `xRetry.v3` source, adapted to xUnit v3.
- Analyzer packaging for xUnit v3 serialization support.

## Usage Notes

- This is the FclEx xUnit test helper package; there is no separate xUnit v2 package.
- The package references xUnit v3 assert and extensibility packages.

## Logging to Test Output

Configure test output through the logging builder:

```csharp
using FclEx.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddXunit(consoleFallback: false));
using var serviceProvider = services.BuildServiceProvider();
serviceProvider.GetRequiredService<ILoggerFactory>()
    .CreateLogger("example")
    .LogInformation("Written to the current xUnit test output");
```

The parameterless-output overload resolves `TestContext.Current.TestOutputHelper` for each
message. Overloads also accept a fixed `ITestOutputHelper` or a resolver delegate.
With `consoleFallback: true` (the default), messages go to the console if test output is
unavailable; with `false`, those messages are dropped. Factory filters still apply.

For an existing factory, use `factory.AddXunit(...)`. Each builder or factory call adds
a provider, so repeated registration can write a message more than once.
