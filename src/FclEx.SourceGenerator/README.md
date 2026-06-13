# FclEx.SourceGenerator

Shared source generator used by FclEx packages.

## What Is Included

- Incremental generator infrastructure for FclEx-generated helper APIs.
- Source metadata helpers.
- Shared string and analyzer-config utilities.
- ABP global using source generation.
- Shared `SourceBuilder` support used by package-specific generators.

## Usage Notes

- This project is primarily consumed as an analyzer by other FclEx packages.
- It is not intended as a standalone runtime dependency.
- Generated files are emitted during package builds for projects that import the build helper props.
