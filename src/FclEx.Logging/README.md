# FclEx.Logging

Logging helpers built on `Microsoft.Extensions.Logging`.

## What Is Included

- Logger property scopes through `LoggerProperties`, `LoggerProperty`, and `LazyLoggerProperty`.
- Extensions for adding scoped properties to `ILogger`.
- Helpers for creating loggers from service providers, factories, and providers.
- Null-logger fallback helpers.
- Operation timing and operation error logging helpers.
- `LogException` and common log property names.
- Service collection helpers for removing logging registrations.
- `LogPropertyIgnoreAttribute` for excluding members from log-property extraction.

## Target Frameworks

By default this project targets `netstandard2.0`, `net472`, `net8.0`, `net9.0`, and `net10.0`.

## Dependencies

- `Microsoft.Extensions.Logging`
- `FclEx.Options`

## Notes

Property scopes are disposable. Prefer `using` blocks or `using var` when pushing properties to ensure scopes are closed even when an operation fails.
