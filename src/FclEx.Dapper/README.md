# FclEx.Dapper

Dapper and ADO.NET helpers for FclEx.

## What Is Included

- CRUD-oriented `IDbConnection` extensions.
- Transaction helpers and operation wrappers.
- Dynamic-parameter utilities.
- SQL adapter abstractions for provider-specific SQL fragments.
- Type-handler helpers for custom Dapper mappings.

## Usage Notes

- This package depends on Dapper and `FclEx.Core`.
- The APIs stay close to ADO.NET and Dapper concepts rather than introducing a full repository framework.
- Some tests expect local database services to be available.
