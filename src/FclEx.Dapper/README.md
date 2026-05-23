# FclEx.Dapper

Dapper and ADO.NET helpers for common CRUD, transaction, and SQL-generation workflows.

## What Is Included

- Extension methods for `DbConnection`, `DbTransaction`, `DbCommand`, and Dapper `DynamicParameters`.
- Insert, bulk insert, get-by-id, and delete helpers built around generated SQL.
- Transaction helpers for single and multiple connections, including rollback-on-failure behavior.
- SQL adapter abstractions for database-specific SQL syntax.
- Built-in adapters for SQL Server, SQLite, PostgreSQL, MySQL, and MySqlConnector.
- Type handlers for `DateTime` and `Guid`.
- Entity and field metadata helpers used by SQL generation.

## Target Frameworks

By default this project targets `netstandard2.0`, `net472`, `net8.0`, `net9.0`, and `net10.0`.

## Dependencies

- `Dapper`
- `FclEx.Core`

## Notes

The CRUD helpers depend on entity metadata inferred from type and property information. For provider-specific SQL, pass an `ISqlAdapter` when the default adapter cannot infer the desired dialect.
