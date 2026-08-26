# FclEx.Dapper Design Principles

This document guides the long-term development and review of FclEx.Dapper. Implementation details may change, but changes to the package should follow these principles.

## Purpose

FclEx.Dapper extends Dapper with focused helpers for recurring database operations such as `InsertAsync<TEntity, TKey>`, `BulkInsertAsync<T>`, `GetAsync<T>`, and `DeleteAsync<T>`. The package should reduce repetitive SQL and ADO.NET code while preserving Dapper's direct, transparent programming model.

EF Core-style change tracking, relationship management, LINQ translation, migrations, and unit-of-work management stay outside the package.

## Principles

1. **Stay close to Dapper and ADO.NET.** Public APIs should compose with existing connection, transaction, command, parameter, and Dapper types. Callers retain control over SQL, connections, transactions, and execution boundaries.

2. **Solve recurring operations with focused extensions.** Each public abstraction must support a concrete Dapper operation or remove repeated code from its implementation. The package should not add a general data-access framework around those extensions.

3. **Model only the metadata that supported operations consume.** Entity mapping exists to generate correct SQL and parameters. Add table, column, key, value-generation, or provider metadata when a supported operation needs it. Leave unrelated ORM concepts out of the model.

4. **Keep mapping contracts explicit and stable.** Generated SQL depends on entity mappings, so mappings used for caching must remain immutable and stable for their intended lifetime. Attribute-based conventions and custom mapping sources must document the semantics they support.

5. **Cache generated SQL on hot paths.** The package should avoid repeated SQL string construction for the same operation and mapping. A cache key must include every input that can change the generated SQL. Inputs with an open-ended value space require bounded caching or direct generation when permanent caching would cause uncontrolled growth.

6. **Keep provider abstractions narrow.** A provider-specific abstraction should express differences required by supported operations, such as identifier quoting, schema support, parameter creation, generated-key retrieval, and batch limits. Model a shared SQL shape once: for example, single-row and multi-row inserts should use the same provider contract when their only difference is the number of value rows. Add capabilities from demonstrated requirements instead of forecasting a complete provider model.

7. **Preserve the contract of bulk operations.** A bulk API exists to reduce database round trips and command overhead. It should execute bounded multi-row batches and must not silently degrade to one command per entity. Provider limits should determine batch boundaries; when an efficient bulk form cannot be expressed, report that limitation explicitly instead of hiding a materially different execution strategy.

8. **Keep Dapper global state outside the core CRUD path.** Core extensions should not modify `SqlMapper` type maps, type handlers, settings, or other process-wide state. Features that require such changes must use an explicit, opt-in API whose name and documentation state the global effect.

9. **Make behavior visible at the call site.** APIs should expose transaction participation, timeout, cancellation, schema overrides, generated-key behavior, and provider-specific choices when they affect results or side effects. Avoid hidden connection ownership, implicit transactions, assembly scanning, and startup mutation.

10. **Prefer small compatible changes.** New APIs should work across the package's target frameworks and supported providers. Expand an abstraction when current behavior requires the expansion, and preserve a smaller design when it meets the same need.

11. **Use evidence for performance and complexity decisions.** Tests should verify SQL correctness, cache isolation, concurrency behavior, batching, and provider differences. Benchmarks should support changes made for allocation or throughput reasons.

## Change Review

Before accepting a change:

1. Name the supported Dapper operation or repeated code that requires it.
2. Identify the metadata and provider behavior that the operation consumes.
3. Confirm that cache keys cover every value that changes generated SQL.
4. Keep process-wide Dapper configuration separate from the CRUD implementation.
5. Reject abstractions that add lifecycle, ownership, or ORM semantics without a current use in the package.
6. Confirm that bulk operations remain bounded multi-row operations and do not introduce per-entity command execution.
7. Verify the behavior with focused tests and use benchmarks for performance claims.
