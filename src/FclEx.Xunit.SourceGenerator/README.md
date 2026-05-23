# FclEx.Xunit.SourceGenerator

Source generator for xUnit v2 serialization helpers.

## What Is Included

- An incremental source generator for `XunitSerializableAttribute`.
- Shared source-generation utilities linked from `FclEx.Core` and `FclEx.SourceGenerator`.
- Generated code that supports xUnit serialization patterns used by `FclEx.Xunit`.

## Target Frameworks

This project targets `netstandard2.0`.

## Dependencies

- `Microsoft.CodeAnalysis.CSharp`
- `Microsoft.CodeAnalysis.Analyzers`

## Notes

This generator is packaged through `FclEx.Xunit` as an analyzer. It is not intended to be referenced as a normal runtime assembly.
