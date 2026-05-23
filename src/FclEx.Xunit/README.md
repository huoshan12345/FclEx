# FclEx.Xunit

xUnit v2 test helpers for FclEx-based test projects.

## What Is Included

- `AssertEx` helpers for member equality, enum equality, date precision, boolean assertions, and collection checks.
- Conditional fact and theory attributes.
- Local-only fact and theory attributes.
- Test output helpers and a `TextWriter` bridge for `ITestOutputHelper`.
- `XunitLogger` and logging provider integration for Microsoft logging.
- Test helper utilities for environment, build type, OS, and runner detection.
- Testing sequence helpers for verifying enumeration behavior.
- `XunitSerializableAttribute` and related source-generated serialization support.

## Target Frameworks

By default this project targets `netstandard2.0`, `net472`, `net8.0`, `net9.0`, and `net10.0`.

## Dependencies

- `xunit`
- `FclEx.Logging`
- `FclEx.SourceGenerator` as an analyzer
- `FclEx.Xunit.SourceGenerator` as an analyzer

## Notes

This package is marked as a non-test project so it can be referenced by test projects without being discovered as a test assembly.
