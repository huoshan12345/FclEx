# FclEx.Dapper

Dapper and ADO.NET helpers for FclEx.

## What Is Included

- CRUD-oriented `IDbConnection` extensions.
- Local transaction helpers and operation wrappers.
- Dynamic-parameter utilities.
- SQL adapter abstractions for provider-specific SQL fragments.
- Type-handler helpers for custom Dapper mappings.
- An explicit `IEntityMappingSource` contract for CRUD SQL generation and caching.

## Usage Notes

- This package depends on Dapper and `FclEx.Core`.
- The APIs stay close to ADO.NET and Dapper concepts rather than introducing a full repository framework.
- Future changes follow the package's [design principles](DESIGN.md).
- CRUD operations do not scan assemblies or modify Dapper's process-wide type maps and type handlers.
- Pass a `CancellationToken` through `CommandInfo` for connection CRUD, or through the final parameter of transaction CRUD. Transaction callbacks can receive the same token directly.
- Some tests expect local database services to be available.

## Entity Mapping

CRUD operations use `DataAnnotationsEntityMappingSource` by default. It supports `[Table]` including `Schema`, `[Column]`, `[Key]`, `[NotMapped]`, and all `[DatabaseGenerated]` values. Unannotated properties must be readable and writable scalar instance properties; navigation, static, indexer, read-only, and explicitly unmapped properties are excluded.

Implement `IEntityMappingSource` when the application should own its mapping independently of DataAnnotations. The source returns stable immutable `EntityMapping` instances composed from `PropertyMapping` values:

```csharp
var orderMapping = new EntityMapping(
    typeof(Order),
    tableName: "orders",
    properties:
    [
        new(typeof(Order).GetProperty(nameof(Order.Id))!, "order_id", isKey: true,
            valueGeneration: DatabaseValueGeneration.OnInsert),
        new(typeof(Order).GetProperty(nameof(Order.Total))!, "total_amount"),
    ],
    schema: "sales");

IEntityMappingSource mappings = new ApplicationEntityMappingSource(orderMapping);
var commandInfo = new CommandInfo(EntityMappingSource: mappings);
await connection.InsertAsync(order, commandInfo: commandInfo);
```

Here `ApplicationEntityMappingSource` is an application-owned implementation that returns `orderMapping` for `Order` and throws for unknown entity types.

An `IEntityMappingSource` must return the same `EntityMapping` instance whenever the same entity type is requested. SQL caches use mapping identity so multiple mapping sources can safely map one CLR type differently.

FclEx-generated queries alias database columns back to CLR property names, so CRUD operations do not require a global Dapper type map. Applications that configure raw Dapper queries or type handlers own those process-wide settings.

`GuidTypeHandler` and `DateTimeHandler` remain available for explicit registration through Dapper.
