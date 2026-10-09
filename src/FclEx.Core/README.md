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

### Adjacent duplicate removal

Use `DistinctAdjacent` to retain the first element of each consecutive run of equal
elements, or `DistinctAdjacentBy` to compare keys while returning the original elements.
Unlike LINQ's `Distinct` / `DistinctBy`, these methods retain equal values or keys when
they are separated by a different value or key, and preserve source order.

```csharp
using FclEx.Extensions;

new[] { 1, 1, 2, 2, 2, 1 }.DistinctAdjacent(); // [1, 2, 1]
new[] { "one", "two", "four", "five", "six" }
    .DistinctAdjacentBy(item => item.Length); // ["one", "four", "six"]
new[] { "a", "A", "b", "a" }
    .DistinctAdjacent(StringComparer.OrdinalIgnoreCase); // ["a", "b", "a"]
```

Both methods validate required arguments immediately, but enumerate lazily using
constant additional space. Each enumeration makes one pass over the source;
`DistinctAdjacentBy` selects a key exactly once per visited element. An omitted or null
comparer uses the default equality comparer for the element or key type.

### String delimiters

Use `IsSquareBracketed` / `TrimSquareBrackets`, `IsParenthesized` / `TrimParentheses`,
`IsCurlyBracketed` / `TrimCurlyBrackets`, and `IsAngleBracketed` / `TrimAngleBrackets`
for brackets. Quotes use `IsDoubleQuoted` / `TrimDoubleQuotes`,
`IsSingleQuoted` / `TrimSingleQuotes`, and `IsBacktickQuoted` / `TrimBackticks`.
Slash pairs use `IsSlashDelimited` / `TrimSlashes` and
`IsBackslashDelimited` / `TrimBackslashes`.

These methods require a non-null string and check only its first and last characters.
Trimming removes exactly one matching outer pair, preserves inner content and whitespace,
and returns the original string when no matching pair is present. A bare pair encloses an
empty string. Nesting and escape sequences are not parsed.

```csharp
"(value)".IsParenthesized();     // true
"((value))".TrimParentheses();   // "(value)"
"/value/".TrimSlashes();         // "value"
" value ".TrimDoubleQuotes();   // " value "
```

Use the corresponding `WrapWith...` methods to add a new pair:
`WrapWithSquareBrackets`, `WrapWithParentheses`, `WrapWithCurlyBrackets`,
`WrapWithAngleBrackets`, `WrapWithDoubleQuotes`, `WrapWithSingleQuotes`,
`WrapWithBackticks`, `WrapWithSlashes`, and `WrapWithBackslashes`.
These methods always add a pair, preserve content without escaping it, and reject null
with `ArgumentNullException`. Empty strings produce a bare pair.

```csharp
"value".WrapWithParentheses();   // "(value)"
"(value)".WrapWithParentheses(); // "((value))"
"".WrapWithSquareBrackets();    // "[]"
```

For custom delimiters, use `IsWrappedWith`, `WrapWith`, `TrimWrapper`, and
`EnsureWrappedWith`. Each has `string` and `char` overloads. Omitting `close` (or passing
null) uses `open` on both ends; closing brackets are not inferred. String delimiters
must be non-null and non-empty and are matched using `StringComparison.Ordinal`.
Matching requires both complete delimiters without overlap.

`EnsureWrappedWith` returns the original string if it already has the specified outer
pair; otherwise, it adds one pair. The same behavior is available through
`EnsureSquareBracketed`, `EnsureParenthesized`,
`EnsureCurlyBracketed`, `EnsureAngleBracketed`,
`EnsureDoubleQuoted`, `EnsureSingleQuoted`,
`EnsureBacktickQuoted`, `EnsureSlashDelimited`, and `EnsureBackslashDelimited`.

```csharp
"value".WrapWith("/*", "*/");              // "/*value*/"
"/*/".IsWrappedWith("/*", "*/");           // false: overlapping delimiters
"/**/".TrimWrapper("/*", "*/");            // ""
"value".EnsureWrappedWith('(', ')');        // "(value)"
"(value)".EnsureParenthesized();   // "(value)"
```

All methods in this family reject a null source with `ArgumentNullException`, including
the existing specific check and trim methods, which previously threw `NullReferenceException`.
The string overloads reject a null `open` with `ArgumentNullException` and empty delimiters
with `ArgumentException`. The existing specific methods delegate to these general methods.
