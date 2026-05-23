# FclEx.Xunit.v3

xUnit v3 test helpers for FclEx-based test projects.

## What Is Included

- The shared test helper surface from `FclEx.Xunit`, compiled for xUnit v3.
- xUnit v3 assertion and extensibility support.
- Logging integration for writing Microsoft logging output to xUnit test output.
- Conditional and local-only test attributes adapted for xUnit v3.
- Source-generated xUnit serialization support through the v3 generator.

## Target Frameworks

By default this project targets `netstandard2.0`, `net472`, `net8.0`, `net9.0`, and `net10.0`.

## Dependencies

- `xunit.v3.assert`
- `xunit.v3.extensibility.core`
- `FclEx.Logging`
- `FclEx.SourceGenerator` as an analyzer
- `FclEx.Xunit.v3.SourceGenerator` as an analyzer

## Notes

This project links the source files from `FclEx.Xunit` and compiles them with the `FCLEX_XUNIT_V3` constant. Use this package for xUnit v3 projects instead of `FclEx.Xunit`.
