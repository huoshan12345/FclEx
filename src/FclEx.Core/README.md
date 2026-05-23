# FclEx.Core

The foundational package for FclEx. It contains general-purpose extensions, data structures, result helpers, operation pipelines, domain abstractions, and compatibility utilities used by the other packages.

## What Is Included

- Extension methods for common .NET types, including strings, collections, LINQ, reflection, IO, networking, JSON, XML, tasks, and diagnostics.
- `OperationResult` and related helpers for success, failure, exceptions, and input/output tracking.
- `IAction<T>` pipelines for composing, mapping, retrying, and combining operations.
- Domain entity contracts such as `IHasId<T>`, soft-delete interfaces, common entity base types, and entity change helpers.
- Collection implementations and comparers, including ordered collections, heap helpers, bytewise comparers, and member-based comparer builders.
- Serialization helpers for `System.Text.Json` and XML.
- Utility types for disposables, lazy values, caching, paging, console tables, regexes, runtime checks, and source building.
- Combinatorics collections for combinations, permutations, and variations.

## Target Frameworks

By default this project targets `netstandard2.0`, `net472`, `net8.0`, `net9.0`, and `net10.0`.

## Dependencies

- `System.Text.Json`
- Compatibility packages for older target frameworks.
- `FclEx.SourceGenerator` as an analyzer.

## Notes

This package is intentionally broad. Prefer using the focused extension methods and small utility types directly rather than treating it as an application framework.
