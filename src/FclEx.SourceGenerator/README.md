# FclEx.SourceGenerator

Internal source generator used by several FclEx packages.

## What Is Included

- An incremental generator that emits common extension methods and helper overloads.
- Generated sources for hashing, numeric helpers, tuple helpers, string builder helpers, event handlers, bytes helpers, and Unicode scalar helpers.
- Generated overloads for dependency-injection factory helpers.
- Generated xUnit helper sources used by test packages.
- Small source-generation infrastructure types such as `SourceInfo` and `SourceBuilder`.

## Target Frameworks

This project targets `netstandard2.0`.

## Dependencies

- `Microsoft.CodeAnalysis.CSharp`
- `Microsoft.CodeAnalysis.Analyzers`
- `AngleSharp`

## Notes

This project is usually referenced as an analyzer by other FclEx packages. It is not intended to be consumed directly as a runtime library.
