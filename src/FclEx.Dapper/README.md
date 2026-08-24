# FclEx.Dapper

Dapper and ADO.NET helpers for FclEx.

## What Is Included

- CRUD-oriented `IDbConnection` extensions.
- Local transaction helpers and operation wrappers.
- Dynamic-parameter utilities.
- SQL adapter abstractions for provider-specific SQL fragments.
- Type-handler helpers for custom Dapper mappings.
- An explicit `IEntityMappingSource` contract shared by CRUD SQL generation and Dapper column mapping.

## Usage Notes

- This package depends on Dapper and `FclEx.Core`.
- The APIs stay close to ADO.NET and Dapper concepts rather than introducing a full repository framework.
- FclEx does not scan loaded assemblies or register Dapper type maps and type handlers automatically.
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

## Explicit Dapper Column Mapping

FclEx CRUD queries alias database columns back to CLR property names and do not require a global Dapper type map. Register one only when raw Dapper queries should also use the selected entity mapping:

```csharp
var dapperRegistration = DapperHelper.CreateConfiguration()
    .UseEntityMappingSource(mappings)
    .AddColumnMapping<Order>()
    .Apply();
```

Keep the returned `FclExDapperRegistration` alive while the mappings are required and dispose it during application or test teardown. Applying the same FclEx configuration is reference counted. If another component already owns a custom type map, `Apply()` throws before changing any selected mapping; pass `KeepExisting` or `Replace` explicitly to choose a different conflict policy.

`GuidTypeHandler` and `DateTimeHandler` remain available for explicit registration through Dapper, but FclEx does not install them as process-wide defaults.
