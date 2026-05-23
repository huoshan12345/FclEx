# FclEx.Xunit.v3.SourceGenerator

Source generator for xUnit v3 serialization helpers.

## What Is Included

- The xUnit serialization generator compiled with the `FCLEX_XUNIT_V3` constant.
- Shared source-generation utilities from `FclEx.Core` and `FclEx.SourceGenerator`.
- Linked generator implementation from `FclEx.Xunit.SourceGenerator`.

## Notes

This generator is packaged through `FclEx.Xunit.v3` as an analyzer. It exists separately so xUnit v3-specific generated code can be emitted without affecting xUnit v2 consumers.
