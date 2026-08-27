# FclEx.Dapper

FclEx.Dapper adds focused, cache-aware CRUD and transaction helpers to Dapper while keeping connections, transactions, SQL providers, and execution boundaries visible to the caller. It is not an ORM: it does not provide change tracking, relationship management, LINQ translation, schema migrations, repositories, or a unit of work.

## Installation

Install FclEx.Dapper and the ADO.NET provider used by the application:

```shell
dotnet add package FclEx.Dapper
dotnet add package Microsoft.Data.Sqlite
```

The package targets `net472`, `netstandard2.0`, `net8.0`, `net9.0`, and `net10.0`. It references Dapper and FclEx.Core, but it does not add a database provider dependency.

## Quick Start

Map an entity with DataAnnotations:

```csharp
[Table("widgets")]
public sealed class Widget
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string Name { get; set; } = "";
}
```

Use a normal provider connection. The schema must already exist; the `CREATE TABLE` below is included only to make the example runnable:

```csharp
using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
await connection.ExecuteAsync(
    "CREATE TABLE widgets (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL)");

var id = await connection.InsertAsync(new Widget { Name = "first" });
var widget = await connection.GetAsync<Widget>(id);

await connection.BulkInsertAsync(
[
    new Widget { Name = "second" },
    new Widget { Name = "third" },
]);

await connection.DeleteAsync<Widget>(id);
```

`InsertAsync<TEntity>` returns the generated key as `long`. Use `InsertAsync<TEntity, TKey>` when the generated key has another type.

## Providers and SQL Adapters

FclEx.Dapper recognizes these provider connection types when the corresponding provider package is installed:

| Database | Provider package | Built-in adapter | Generated-key SQL |
| --- | --- | --- | --- |
| SQL Server | `Microsoft.Data.SqlClient` | `SqlServerAdapter` | `OUTPUT INSERTED` |
| PostgreSQL | `Npgsql` | `NpgsqlAdapter` | `RETURNING` |
| SQLite | `Microsoft.Data.Sqlite` | `SqliteAdapter` | `last_insert_rowid()` |
| MySQL | `MySql.Data` | `MySqlAdapter` | `LAST_INSERT_ID()` |
| MySQL/MariaDB | `MySqlConnector` | `MySqlConnectorAdapter` | `LAST_INSERT_ID()` |

Adapter resolution examines the runtime connection type and its base types. Register an adapter for a custom or wrapped connection type when it cannot be recognized automatically:

```csharp
DapperHelper.RegisterSqlAdapter<CustomConnection>(new CustomSqlAdapter()); // add or replace

var added = DapperHelper.TryRegisterSqlAdapter<CustomConnection>(new CustomSqlAdapter());
```

`TryRegisterSqlAdapter` returns `false` when the exact connection type is already registered. A registered adapter must keep its SQL-affecting behavior stable because generated SQL is cached by adapter instance. Schema support follows `ISqlAdapter.SupportsSchemas`; unsupported adapters ignore mapped and per-call schemas.

Table, schema, and column names supplied through mappings or method arguments must come from trusted application configuration, not end-user input. Adapters quote and escape identifiers, but identifier names cannot be parameterized like data values.

## Entity Mapping and Key Limits

`DataAnnotationsEntityMappingSource` is used by default. It supports:

- `[Table]`, including `Schema`
- `[Column]`, including column aliases and provider store type names
- `[Key]`
- `[NotMapped]`
- `[DatabaseGenerated]` with `None`, `Identity`, or `Computed`

By convention, public readable and writable scalar instance properties are persistent. Static properties, indexers, read-only properties, navigation properties, and explicitly unmapped properties are excluded. A non-scalar property must declare an explicit mapping attribute to be included.

`GetAsync<T>` and `DeleteAsync<T>` require exactly one mapped key. Generated-key return from `InsertAsync` requires exactly one generated key. Composite-key lookup and deletion are not supported.

Implement `IEntityMappingSource` when mappings should be independent of DataAnnotations. A source must return the same immutable `EntityMapping` instance whenever the same entity type is requested because mapping identity participates in SQL cache keys:

```csharp
var mapping = new EntityMapping(
    typeof(Widget),
    "widgets",
    [
        new(typeof(Widget).GetProperty(nameof(Widget.Id))!, "Id", true,
            DatabaseValueGeneration.OnInsert),
        new(typeof(Widget).GetProperty(nameof(Widget.Name))!, "Name"),
    ]);

var options = new CommandOptions { EntityMappingSource = new ApplicationMappingSource(mapping) };
var id = await connection.InsertAsync(new Widget { Name = "mapped" }, commandOptions: options);
```

`ApplicationMappingSource` in this example is an application-owned `IEntityMappingSource` implementation.

## Commands, Connections, and Transactions

`CommandOptions` carries the command timeout, local transaction, adapter override, mapping source, and cancellation token. The same options shape is accepted by connection and transaction CRUD methods.

CRUD helpers record the connection's initial state. A connection opened by the helper is closed before the operation returns; a connection supplied already open remains open. Transaction extension methods bind the receiver transaction to the generated command.

`ExecuteInTransactionAsync` starts a local transaction with `ReadCommitted` by default, commits after a successful callback, and attempts rollback after callback or commit failure. If both the operation and rollback fail, an `AggregateException` contains both exceptions.

## Bulk Inserts and SQL Caching

`BulkInsertAsync` emits bounded multi-row INSERT commands; it does not silently execute one command per entity. A batch contains at most 500 rows and may be smaller because of provider row or parameter limits. Multiple rows with no insertable properties are rejected when the adapter cannot express an efficient bulk form.

Canonical CRUD command text is cached by adapter instance, immutable mapping identity, operation shape, and batch row count. Per-call schema and adapter overrides do not enter the process-wide cache. This avoids repeated SQL string construction without permanently retaining open-ended override values.

## Explicit Generated Keys

Use `InsertWithExplicitGeneratedKeysAsync` or `BulkInsertAsync(..., includeAutoKey: true)` only when importing values for keys normally generated by the database.

These operations do not advance or reset provider identity, sequence, or auto-increment state. The caller must maintain that state so later database-generated keys do not conflict with explicitly inserted values.

## Dapper Global State and Type Handlers

Core CRUD operations do not scan assemblies or modify Dapper's process-wide type maps, type handlers, or settings. Generated queries alias database columns back to CLR property names, so they do not require a global Dapper type map.

`Dapper.GuidTypeHandler` and `Dapper.AssumeUtcDateTimeTypeHandler` are optional helpers. Registering either through `SqlMapper.AddTypeHandler` changes Dapper process-wide state and remains the application's responsibility.

See [DESIGN.md](DESIGN.md) for the principles governing future changes.
