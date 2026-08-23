# FclEx.Dapper

Dapper and ADO.NET helpers for FclEx.

## What Is Included

- CRUD-oriented `IDbConnection` extensions.
- Local transaction helpers and operation wrappers.
- Dynamic-parameter utilities.
- SQL adapter abstractions for provider-specific SQL fragments.
- Type-handler helpers for custom Dapper mappings.

## Usage Notes

- This package depends on Dapper and `FclEx.Core`.
- The APIs stay close to ADO.NET and Dapper concepts rather than introducing a full repository framework.
- FclEx does not scan loaded assemblies or register Dapper type maps and type handlers automatically.
- Some tests expect local database services to be available.

## Explicit Column Mapping

Register DataAnnotations column mappings at application startup only for the entity types or assemblies that need them:

```csharp
var dapperRegistration = DapperHelper.CreateConfiguration()
    .AddColumnMappingsFromAssembly(typeof(MyEntity).Assembly)
    .Apply();
```

Keep the returned `FclExDapperRegistration` alive while the mappings are required and dispose it during application or test teardown. Applying the same FclEx configuration is reference counted. If another component already owns a custom type map, `Apply()` throws before changing any selected mapping; pass `KeepExisting` or `Replace` explicitly to choose a different conflict policy.

`GuidTypeHandler` and `DateTimeHandler` remain available for explicit registration through Dapper, but FclEx does not install them as process-wide defaults.
