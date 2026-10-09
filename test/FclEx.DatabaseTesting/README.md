# Shared Database Test Infrastructure

`FclEx.DatabaseTesting` is a non-shipping class library used independently by Dapper and EF Core tests. It supports the test framework matrix, including Windows `net472`, and has no EF Core or executable test-project dependency.

## Responsibilities

1. Share entities and configuration types without duplicating their definitions.
2. Resolve the physical database, schema scenario, login, and SQLite file before building a native connection string.
3. Own a temporary SQLite database for each fixture, initialize its shared schema, and delete its files when the fixture ends.
4. Define normal provider selection independently of remote provisioning selection.

EF-specific models, model caches, and provider configuration remain in `FclEx.EfCore.Tests`. Common xUnit assertions and case data are linked from `test/DatabaseTests` into both test projects. Configuration continues to come from `FclEx.Core.Tests`; do not copy decrypted credentials into this library.

## Select providers and run filtered tests

Run these commands from the repository root. `FCLEX_TEST_DATABASES` accepts comma-separated driver names, ignoring case and duplicate entries. Invalid, empty, or unavailable names fail instead of silently reducing coverage.

```powershell
$env:FCLEX_TEST_DATABASES = 'Sqlite'
dotnet test --project test/FclEx.Dapper.Tests/FclEx.Dapper.Tests.csproj -c Release --framework net472 --no-restore -- --filter-class '*DbConnectionExtensionsTests'
dotnet test --project test/FclEx.EfCore.Tests/FclEx.EfCore.Tests.csproj -c Release --framework net10.0 --no-restore -- --filter-class '*BasicTests' '*SchemaDbContextTests' '*SqliteSchemaTests'
```

Neither command requires running the other project first. Restore once when dependencies or target frameworks change. Omit `--framework` to run all targets enabled for the project and host. Add `--no-build` only after building the current changes.

For the modern targets, select all supported drivers with:

```powershell
$env:FCLEX_TEST_DATABASES = 'Sqlite,SqlServer,Npgsql,MySqlConnector,MySql,Oracle'
```

Exclude `Oracle` for `net472`, where the current build does not support that driver. `net472` runs on Windows only. MySql.Data and MySqlConnector use the same server but have separate behavioral cases. EF's `MySql` case uses MySql.EntityFrameworkCore; its `MySqlConnector` case uses Pomelo or Microting according to the target framework.

Unset the override to restore the defaults:

```powershell
Remove-Item Env:FCLEX_TEST_DATABASES -ErrorAction SilentlyContinue
```

Default selection is defined in DatabaseTestSettings and can change independently of this README. Every supported driver is eligible for CI coverage on each supported operating system; splitting drivers between jobs is an execution-time choice, not a compatibility rule. Use FCLEX_TEST_DATABASES for focused runs. Provider-specific tests retain their original driver constraints; independent specialized SQLite tests retain their own local setup.

## SQLite schema and lifetime

The authoritative shared relational model remains [TestDbContext](../FclEx.EfCore.Tests/FclEx/EfCore/TestDbContext.cs). [Schemas/Sqlite.sql](Schemas/Sqlite.sql) is generated from that model and embedded in this library. Both fixtures execute the resource directly; Dapper never loads EF Core to create tables.

Each fixture creates an absolute, unique file path below the system temporary directory. Connections enable foreign keys, disable pooling, and use a 30-second busy timeout. Initialization enables WAL. Multiple connections in a fixture share its file; other fixtures and processes have different files. SQLite still serializes writers, so tests must dispose connections and transactions promptly. Dispose connections and contexts before disposing the environment so cleanup can remove the database and WAL files.

SQLite participates in the existing common tests with the null-schema scenario. The existing default-namespace test checks its absence of server schemas and uses the same local file; its login option has no effect for SQLite. Attached databases are outside this matrix. Existing remote schema and login cases remain intact.

After changing the EF model, regenerate the resource through the explicitly enabled exporter:

```powershell
$env:FCLEX_TEST_DATABASES = 'Sqlite'
$env:FCLEX_SQLITE_SCHEMA_OUTPUT = Join-Path (Get-Location) 'test/FclEx.DatabaseTesting/Schemas/Sqlite.sql'
dotnet test --project test/FclEx.EfCore.Tests/FclEx.EfCore.Tests.csproj -c Release --framework net10.0 --no-restore -- --filter-method '*ExportSchema' --explicit only
Remove-Item Env:FCLEX_SQLITE_SCHEMA_OUTPUT
```

The exporter generates SQL without opening a database. Rebuild the shared library after export to embed the new script. `SqliteSchemaTests.GeneratedSchema_MatchesSharedResource` compares the script with the current model on each EF target and catches drift. Inspect generated changes rather than manually maintaining a second schema.

## Remote provisioning and isolation

Remote resources keep the existing project/framework/OS names and schema/default-login cases. Fixtures own their `DatabaseEnvironment`. `TestDatabaseEnvironment.Resolve` maps MySQL's schema scenario to the selected database and Oracle's owner scenario to the actual login. `TestDatabaseTarget` builds the appropriate native connection string without global credential caching. `TestDbContext` consumes the resolved string without rewriting it.

Normal startup does not rebuild shared remote schemas or users. `SynchronizeIdentitySequenceAsync` first checks the mapped table through the production TableExistsAsync extension and returns zero when it is absent; otherwise it retains the existing PostgreSQL identity-sequence repair. `SelectedDrivers` identifies this process's normal run selection. The explicitly enabled `TestDbContextTests.SetupDatabase` remains the destructive provisioning operation; its engine matrix is independent of the normal provider override and excludes SQLite and the duplicate MySql.Data engine.

Remote names isolate projects, frameworks, operating systems, and schema scenarios. Common cases are designed for parallel execution using independent rows. Environment naming does not provide a cross-process lock for two runs of the same matrix entry.

## Truncate test isolation

Truncate tests use the minimal [TruncateEntities](FclEx/Databases/TruncateEntities.cs)
rather than the common fixture entities. Tables contain only an integer key, a value
column when filtering or keyless behavior needs it, and a parent key for cascade tests.
EF uses a dedicated `TruncateDbContext` model; these entities are never added to the
shared `TestDbContext` schema. Dapper reuses the same simple entities and session mappings.

SQL Server uses local `#` tables, PostgreSQL uses `pg_temp`, and MySQL and SQLite use
connection-local temporary tables. Each test creates only the tables it needs and
keeps its connection open until all contexts have finished. MySqlConnector connections
retain the provider-required `AllowUserVariables=True;UseAffectedRows=False` settings.

Initialization does nothing unless Oracle is selected. For Oracle it idempotently
creates only the dedicated parent and child tables per schema, with an
`ON DELETE CASCADE` foreign key. Those two tables persist between runs; cascade tests
exclusively lease them and clean their rows, without a multi-table pool. The lease is
process-local and does not isolate simultaneous runs of the same project/framework/OS/schema.

Other Oracle cases create uniquely named ordinary tables immediately before each test
and drop them on session disposal. Oracle native identity behavior requires ordinary
tables rather than connection-local temporary identity tables. Closed-connection and
ordinary-schema cases on every driver also use uniquely named, on-demand ordinary
tables, which are dropped afterward. No truncate test modifies the common fixture tables.

Cascade result tests on PostgreSQL and Oracle insert only a parent and a child and check
that both row counts become zero. Unsupported option combinations retain their
closed-connection validation coverage. MySql.Data retains its existing truncate-test
skip for its asynchronous timeout cleanup issue; MySqlConnector remains covered.

### Coverage and execution cost

[TruncateTestCases](FclEx/Databases/TruncateTestCases.cs) is the single definition of the supported option, rollback, cascade, and SQLite driver cases. The result matrix exercises every supported identity/cascade combination. Unsupported combinations are tested once per driver with closed connections, without creating or seeding tables. Driver-restricted matrices supply an explicitly skipped row when no selected driver applies.

Dapper and EF implement truncation independently, so each retains its own result, cancellation, and rollback coverage. Shared session isolation is tested only in Dapper.Tests. Both projects use the same [DerivedSqliteConnection](FclEx/Databases/DerivedSqliteConnection.cs) for connection-subclass regressions. EF's four default entry points share one session, as do its four explicit entry points for each supported option combination; each still verifies identity behavior. Cancellation seeds once and exercises all eight EF overloads. Named shared-type, schema, and derived-connection tests focus on their own behavior instead of repeating identity checks. Tests own final row cleanup; seeding clears between calls without another cleanup after every assertion group. Rejection of CLR-type access to named shared entities is checked once without database commands.

Dapper cancellation and malformed-name guards run on the connection holding the seeded temporary table, so the unchanged-row assertion checks the actual operation target. Separate closed-connection calls retain connection-state coverage.

## Compatibility failures

Keep common tests and their assertions shared across applicable providers. First diagnose failures as library defects, compensable provider gaps, incorrect assumptions, environment failures, or inherent database differences. Implement compensable gaps in the production package and retain permanent regressions. Use a provider-specific path only for a real semantic difference, and `Assert.Skip` only for unsupported behavior.

The SQLite TEXT-to-`DateTimeOffset` gap is handled by the production default Dapper type handler. It preserves explicit offsets and tick precision, assumes UTC for offsetless text or unspecified `DateTime`, and respects application handlers. Common tests cover parameter binding across drivers and EF-created timestamp materialization through Dapper. Schema creation and value compatibility are checked separately.
