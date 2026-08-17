# FclEx.Core

The foundational package for FclEx.

## What Is Included

- Extension methods for common .NET types, including strings, collections, LINQ, reflection, IO, networking, JSON, XML, tasks, diagnostics, and random test-data generation.
- `OperationResult` and related helpers for success, failure, exceptions, elapsed time, and input/output tracking.
- `IAction<T>` pipelines for composing, mapping, retrying, and combining operations.
- Domain entity contracts such as `IHasId<T>`, soft-delete interfaces, common entity base types, and entity change helpers.
- Collection implementations such as `BiDictionary`, `BPlusTreeDictionary`, `Deque`, `Heap`, ordered lists, ordered indexes, and multi-value dictionaries.
- Comparers and comparer builders, including key/member comparers, delegate comparers, enumerable comparers, bitwise and interop-marshal equality comparers, and equality-comparer builders.
- Serialization helpers for `System.Text.Json` and XML.
- Utility types for disposables, lazy values, paging, console tables, regexes, runtime checks, expression building, and source building.
- Combinatorics collections for combinations, permutations, and variations.

## Usage Notes

- This package is intentionally broad and is referenced by most other FclEx packages.
- Prefer using focused extension methods and small utility types directly rather than treating the package as an application framework.
- Several APIs backfill newer .NET conveniences on older target frameworks.
