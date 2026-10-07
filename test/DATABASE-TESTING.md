# Multi-Database Testing Requirements and Change Plan

Status: Phases 1–4 implemented and validated with filtered tests on Windows. Remote provisioning selection is independent of run selection; remote schema fingerprints and concurrent-run slots remain deferred. Sections 2 and 5 retain the original baseline and change plan; Section 7 records implementation evidence.

## 1. Purpose

The two projects use real databases to verify their respective libraries against a common behavioral contract. Running the same applicable tests against different providers must expose missing functionality, inconsistent conversions, SQL differences, and transaction or connection behavior that a single-provider test run would miss.

Where a provider lacks a small feature that FclEx can reasonably supply, the intended outcome is a production-library improvement and a permanent regression test. Provider limitations are therefore part of the work these tests are meant to discover and resolve. They must not automatically become skipped cases or test-only workarounds.

For example, the initial SQLite experiment found that reading `EntityHasStates` through Dapper fails when a SQLite TEXT value is materialized as `DateTimeOffset`. Supporting this conversion belongs within the evaluation and improvement scope of `FclEx.Dapper`. Successful schema creation alone does not satisfy the requirement.

## 2. Baseline Architecture Before This Change

1. `FclEx.EfCore.Tests` references `FclEx.Dapper.Tests` to reuse database configuration, connection helpers, fixtures, and entities. The reverse dependency is unsuitable because EF Core tests do not support `net472`.
2. Dapper tests target `net8.0`, `net9.0`, and `net10.0`, with `net472` added on Windows. EF Core tests target the modern frameworks only. Repository conditions currently omit `net9.0` on Windows CI, and Oracle support is conditional on the target framework.
3. Most integration tests share the entities in `FclEx.Dapper.Tests/FclEx/Databases/Entities.cs` and the relational model in `FclEx.EfCore.Tests/FclEx/EfCore/TestDbContext.cs`. Specialized tests also have their own tables or models.
4. `TestDbContextTests.SetupDatabase`, available under `NET10_0`, is an explicitly enabled local provisioning operation. It creates databases, namespaces, users, and tables for multiple project/framework/OS combinations. Normal remote test runs use those previously prepared resources.
5. Remote SQL Server, MySQL, PostgreSQL, and Oracle environments are already available. Their structures normally need recreation only when the test model changes.
6. Resource names incorporate the test project, runtime major version, and operating system. The Windows and Linux jobs in `.github/workflows/build.yml` run the solution independently and allow concurrent test execution.
7. Shared configuration originates in `FclEx.Core.Tests`. Its settings files, including the decrypted configuration when available, are copied to the output directories of referencing test projects. Credentials must continue to come from this configuration without being copied into documentation or committed artifacts.
8. Provider selection currently lives in `DapperTestsFixture.GetDbProviderTypes()`. Windows CI selects PostgreSQL; Linux CI selects SQL Server, MySqlConnector, and Oracle, subject to build support. The inspected local selection enables SQL Server. This is a run selection, not the complete set of intended providers.
9. SQLite already has dedicated Dapper and EF Core tests with local initialization, but it is absent from the common provider matrix. The Dapper FluentMigrator helper creates a limited set of tables rather than the complete shared model.

## 3. Requirements

### R1. Independent execution

Either test project must run independently with a class or method filter. Dapper tests, including `net472`, must initialize SQLite without loading EF Core or waiting for an EF Core test to execute first. Solution-level parallel execution must not introduce a setup ordering dependency between the projects.

### R2. Shared schema with clear ownership

Maintain one authoritative definition of the shared relational structure. Reuse the common entities and applicable tables across both projects. Preserve table and column names, keys, generated values, indexes, foreign keys, nullability, and provider-specific store types.

An entity shared with EF does not automatically have a valid Dapper CRUD mapping: EF conventions and Dapper's explicit mapping requirements must each be respected. Dedicated models for relationship, metadata, or provider-specific tests may remain separate.

### R3. Real provider coverage

Exercise SQL Server, PostgreSQL, MySQL, Oracle, and SQLite through the supported drivers and frameworks. Treat database engine and client provider as different dimensions: MySql.Data and MySqlConnector use the same engine but still need their own applicable behavioral coverage.

The supported matrix is constrained by actual driver/framework compatibility. It does not require EF Core on `net472`, Oracle on a target that currently excludes its driver, or `net472` on Linux. Selected runs must report which providers were exercised or intentionally excluded.

### R4. Production compatibility improvements

Use the same behavioral expectations across providers wherever the public API promises equivalent behavior. Cover parameter binding, result materialization, scalar conversions, generated keys, batch insertion, transactions, cancellation, and connection ownership as applicable.

When a common case fails:

1. Determine whether the cause is a library defect, a compensable provider gap, an incorrect test assumption, an environment failure, or an inherent database semantic difference.
2. Keep a valid failing reproducer in the matching test suite.
3. Implement a compensable gap in the appropriate production package, including `FclEx.Dapper`, rather than making the test silently compensate for it.
4. Verify the fix against the affected real provider and add checks that protect other supported providers from regressions.
5. Use provider-specific expectations only for intentional, documented semantic differences. Missing functionality that FclEx is intended to provide is not a reason to skip the test.

### R5. Isolation and concurrency

Preserve isolation between test projects, frameworks, operating systems, and namespaces. Tests within the same environment must identify their own data or use a dedicated resource when an operation affects shared state.

Operations involving whole-table cleanup, explicit identity values, sequence repair, or destructive DDL require particular attention. Per-process environment naming does not by itself make these operations safe within a process, or across two concurrent workflows using the same environment name.

### R6. Separate provisioning from normal execution

Retain the existing pre-provisioned remote database model. Normal test startup must not drop remote databases, recreate users, or rebuild remote schemas. Provisioning remains an explicit operation. Truncate fixtures may create missing dedicated tables idempotently; they retain existing definitions and data until the owning test cleans its rows.

SQLite is local and disposable: its structure must be created automatically before the relevant tests run, with a defined cleanup lifetime. Local initialization must not require administrative access to a remote database.

### R7. Accurate database and identity semantics

Distinguish the physical database/catalog, object schema or owner, login credentials, and namespace scenario under test. Explicit-schema access and a user's default namespace are separate behaviors and must remain separately testable.

| Engine | Relevant concepts | Coverage to preserve |
| --- | --- | --- |
| SQL Server | Database, object schema, server login, database user, default schema | Default access, explicitly qualified schema, user default schema |
| PostgreSQL | Database, schema, login role, search_path | Default access, explicitly qualified schema, role default search path |
| MySQL | Database and schema are synonymous; account is separate | Selected database and explicitly qualified database access |
| Oracle | Service/PDB, login user, object owner/schema, session namespace | Owner-qualified access and the namespace associated with the selected login |
| SQLite | Database file or in-memory database; no server-style users or schemas | Common operations without artificial server-schema cases |

SQLite attached databases are outside the initial scope. Schema-ignore or unsupported-feature behavior can have dedicated tests without duplicating the entire CRUD matrix.

### R8. Maintainability and focused validation

Prefer incremental changes that retain working remote tests. Avoid introducing a migration framework or multiple support projects unless they simplify an identified responsibility. Keep public API documentation and regression tests aligned with any production behavior changes.

Use filtered runs during development. Broaden testing after focused checks pass when the change's scope justifies it. Report actual provider/framework coverage and distinguish real integration tests, offline model checks, and exploratory prototypes.

## 4. Design Rationale

### 4.1 Keep EF as the initial shared schema source

Continue using the existing `TestDbContext` model for remote provisioning. For SQLite, export its creation SQL through `Database.GenerateCreateScript()` and store the generated script as an embedded resource available to Dapper tests.

The script is a generated artifact, not a second manually maintained schema. Use a designated EF target, initially `net10.0`, to generate it without opening a database. Provide an explicit regeneration command or helper. A consistency test on that target should compare the normalized generated script with the stored resource and fail with regeneration instructions if it is stale.

Other EF targets should verify that the baseline creates a usable structure and supports the intended behavior; they need not emit byte-for-byte identical SQL forever. Provider upgrades can legitimately change generation details.

Dapper executes the stored SQL through ordinary ADO.NET and remains independent of EF at runtime. EF tests may continue to use `EnsureCreatedAsync()` for their local databases. Do not regenerate the script as a side effect of an ordinary test that other concurrently running tests depend on.

### 4.2 Give SQLite an explicit resource lifetime

Start with a unique temporary file per shared fixture, using an absolute path. Initialize it once before exposing connections or contexts, and retain that target for the fixture's lifetime. Release connections and any retained pooled resources before removing the fixture's files.

Configure foreign-key enforcement and a bounded busy timeout. Evaluate WAL with the actual concurrent workload. If specific tests require stronger isolation or conflict under parallel writes, give those tests dedicated databases or narrowly restrict their parallelism. Do not replace every test's transaction behavior with an outer rollback transaction.

Named shared in-memory databases remain a valid alternative when a keep-alive connection owns their lifetime. Plain `Data Source=:memory:` is suitable when a test deliberately shares one open connection, but not when independent connections must see the same data. Existing specialized SQLite helpers need not all be replaced.

### 4.3 Generate cases from the behavior being tested

Replace unconditional provider/schema cross joins with explicit case generation. Common CRUD cases include SQLite with one ordinary namespace case. Explicit namespace and default-login-namespace cases are generated only for applicable engines and supported targets.

Provider selection should become configurable without commenting out source lines, while preserving deliberate CI defaults. Reject unknown provider selections and make the effective selection visible. Keep the provisioning engine list separate from the normal run selection.

### 4.4 Resolve physical targets before building connections

Introduce a small resolver that takes a logical test case and returns its physical target and credentials. Keep these responsibilities distinct:

1. Environment identity: test project, framework, OS, and optionally a stable environment slot.
2. Physical target: database/catalog, object schema or owner, or SQLite path.
3. Login: the actual username and password for the requested identity scenario.
4. Test intent: ordinary access, an explicit namespace, or a user's default namespace.

Connection-string construction then translates an already resolved target into the native provider format. It should not reinterpret a parameter named `database` as an Oracle username or silently choose a different identity.

Replace ambiguous `isUser` arguments with named login scenarios. Administrative connections belong to the explicit provisioning path. Preserve the current Oracle login/owner behavior during the first refactor; testing a different login against another owner's objects would be a separate scenario.

Move MySQL and Oracle connection-string rewriting out of `TestDbContext.OnConfiguring`. Prefer resolved connection information and preconfigured context options. Keep the EF schema model cache and necessary provider schema-generation adaptations intact until their behavior is covered by regression tests.

Remove the static `ConnectionStrings` builder cache, or make it instance-scoped with complete identity semantics. Its current key excludes configuration and user identity even though the cached value depends on them. Connection-string construction itself does not justify a global cache.

### 4.5 Extract shared infrastructure only when useful

The initial SQLite change can preserve `EfCore.Tests -> Dapper.Tests`. If the target resolver and lifecycle code warrant a common home, extract a non-shipping ordinary class library, provisionally `FclEx.DatabaseTesting`, referenced by both test projects.

That library would own shared entities, provider/configuration types, environment naming, target resolution, connection creation, and the SQLite schema resource. It must support the required test targets without depending on EF Core or either executable test project. Account explicitly for the inherited MSBuild test settings so the helper is neither discovered as a test project nor packaged for publication.

Keep xUnit case data, assertions, and test classes in the test projects. Keep EF models and provider options in EF tests initially. Both projects can continue obtaining configuration through `FclEx.Core.Tests` and pass it to the helper. A separate EF model library or provisioning executable is optional, not a prerequisite.

## 5. Implementation Plan and Acceptance Criteria

### Phase 1. Establish independent SQLite initialization

1. Add generation and consistency checking for the shared SQLite schema resource.
2. Add fixture-owned SQLite initialization and cleanup without an EF runtime dependency in Dapper tests.
3. Make connections and contexts within a fixture use the same resolved local target.
4. Add SQLite to applicable common cases and remove duplicate server-schema combinations for it.
5. Validate generated keys, GUIDs, nullable values, BLOBs, indexes, foreign keys, transactions, and repeatable resource cleanup.

Acceptance: either project can run selected common SQLite tests independently; Dapper can do so on Windows `net472`; no preceding EF test or remote rebuild is required. Any discovered compatibility failures remain visible and feed Phase 2.

### Phase 2. Close provider functionality gaps

1. Turn the SQLite TEXT-to-`DateTimeOffset` finding into a focused permanent regression test through the intended public access path.
2. Define the conversion contract, including null handling, offsets, precision, supported representations, and failure behavior.
3. Choose the appropriate production implementation. Account for Dapper's process-wide type handlers so a SQLite fix does not change other providers unexpectedly or overwrite a consumer's custom handler.
4. Verify result materialization and parameter binding separately. Extend common coverage to other relevant conversions and retain cross-provider cases.
5. Fix additional compensable gaps revealed by the common matrix, updating public XML documentation and package documentation when behavior changes.

Acceptance: required common behaviors pass because the production library supports them, not because tests skip the provider or manually convert its results. Changes are validated on every supported target affected by the implementation and against representative other providers.

### Phase 3. Clarify connection and namespace design

1. Add logical test cases and physical target resolution with explicit login scenarios.
2. Centralize engine-specific routing currently spread across `ConnectionStrings`, `DapperTestsFixture`, and `TestDbContext`.
3. Remove the incomplete global connection-string cache and context-side connection-string mutation.
4. Add offline resolver tests and real namespace/default-login regression tests for the applicable engines.
5. Separate configurable run selection from provisioning selection.

Acceptance: current remote behavior remains covered, and the selected database, schema/owner, and login can be understood before a connection is opened. Invalid combinations fail explicitly.

### Phase 4. Simplify shared project ownership, if justified

1. Extract the ordinary support library described in Section 4.5.
2. Replace fixture inheritance across test assemblies with composition where needed.
3. Remove the EF test project's reference to the Dapper test project after shared responsibilities have moved.
4. Verify all required targets, provider references, configuration-file copying, and independent filtered execution.

Acceptance: both executable test projects depend on shared infrastructure without depending on each other. This phase may be deferred if the smaller arrangement remains clear and maintainable.

### Phase 5. Strengthen remote environment operations

1. Give provisioning an explicit environment/engine manifest instead of deriving it from the current normal-test provider selection.
2. Optionally record a schema version or fingerprint and fail normal startup with useful instructions when a prepared environment is outdated.
3. Audit operations requiring exclusive access within a shared environment. Keep ordinary common cases parallel and restrict an individual test only when its operation requires it; investigate concrete cross-process conflicts before adding a lease or slot mechanism.
4. Keep destructive provisioning separate from ordinary CI tests and perform it only when explicitly requested.

Acceptance: overlapping runs have a documented isolation strategy, and stale environments are diagnosed without automatic remote destruction. Random per-run remote databases are not required by this plan.

## 6. FluentMigrator Decision

FluentMigrator is technically viable: existing SQLite migration tests already initialize local databases without EF, including on `net472`. However, a full conversion would still require engine-specific user provisioning, type mappings, namespace handling, and consistency between migrations and EF models.

The current workflow rebuilds test structures when the model changes and does not require preservation of test data through a migration history. Retaining EF as the shared schema source and exporting SQLite SQL is therefore the recommended first step.

Reconsider FluentMigrator if incremental upgrades become necessary or the physical schema should be defined independently of EF. In that design, migrations must become the authoritative physical structure and EF must map to it. Do not maintain competing authoritative definitions. Existing focused FluentMigrator tests can remain in place.

## 7. Validation Evidence and Implementation Status

The initial assessment on 2026-10-07 produced the following evidence on Windows:

| Check | Result | Scope |
| --- | --- | --- |
| Dapper `SqliteMigrationTests` and `SqliteBulkInsertTests`, `net472` | 14 passed | Real local SQLite; filtered test run |
| EF `SchemaDbContextTests` and `SchemaModelCacheKeyTests`, `net10.0` | 6 passed | Real SQL Server schema cases plus offline model-cache checks |
| EF 8/9/10 schema export and SQLite read/write prototype | Passed on all three targets | Nine tables; identical generated scripts in this snapshot; identity/GUID operations and multiple connections |
| Separate `net472` prototype executing EF-generated SQL | Passed | Dapper identity/GUID round trips without an EF project reference |
| Dapper materialization of `EntityHasStates` from SQLite | Failed on `DateTimeOffset` | Valid compatibility work item; not evidence that the entire SQLite matrix passes |

Exploratory code is retained locally under the ignored `artifacts/database-design-spike/` directory. The conversion reproducer is available through the schema prototype's `--probe-datetimeoffset` argument. These artifacts are not permanent regression tests or committed implementation.

The initial assessment established feasibility. The implementation and subsequent validation below supersede its pending-work list. Linux execution remains unverified; no remote database rebuild was performed.

For implementation, begin with filtered classes or methods on the affected target. Keep valid failing cases, diagnose their cause, and then broaden coverage when the change warrants it. Do not rerun the entire solution merely to validate this documentation.

### Implemented architecture

1. Both executable test projects reference the ordinary, non-shipping `FclEx.DatabaseTesting` library independently. The EF-to-Dapper-test dependency is removed. Shared xUnit sources are linked into both projects; the helper has no EF or xUnit dependency.
2. The shared EF model remains authoritative. Its nine-table SQLite creation script is embedded in the helper. An explicit net10 exporter regenerates it, and consistency tests currently compare it on all three EF targets. Future provider generation differences require reviewing the baseline or narrowing textual comparison to the designated exporter target while retaining behavioral coverage on every target.
3. Fixtures create unique temporary SQLite files automatically, enable foreign keys and WAL, disable pooling, and remove owned files after connections are disposed. Concurrent connection tests retain all 160 inserted rows; separate environments cannot observe each other's rows.
4. Common CRUD/schema case generators add one ordinary SQLite case and retain every existing remote provider/schema combination. Existing assertions remain in place. Common tests also verify date-time-offset parameter round trips and EF-created timestamp materialization through Dapper.
5. Physical target resolution now distinguishes database, schema scenario, login, and file path. MySQL schema routing and Oracle owner/login resolution happen before connection construction. The incomplete global credential cache and EF context-side connection-string mutation are removed.
6. `FCLEX_TEST_DATABASES` controls filtered run selection with validation. Defaults preserve existing remote selections and add SQLite. Explicit provisioning selection covers the remote engines independently, without duplicate MySQL engine provisioning.
7. `FclEx.Dapper` registers an independent default `DateTimeOffset` handler for native values, `DateTime`, and invariant text, including SQLite TEXT. Explicit offsets and tick precision are preserved; unspecified date-time values and offsetless text assume UTC. Nullable scalar database nulls remain null. Application-supplied GUID and date-time-offset handlers are preserved independently. These registrations affect Dapper process-wide state and are documented in the package README and XML documentation.
8. No FluentMigrator conversion is needed for this change. Existing specialized migration tests remain intact. The helper README provides filtered commands, selection rules, schema regeneration, lifetime rules, and provisioning guidance.

### Filtered implementation validation

All execution below used the provisioned environment on Windows, with local SQLite and real remote providers where selected. Offline resolver and type-handler checks are included in the relevant counts. The checks do not represent a full solution run.

| Check | Result | Scope |
| --- | --- | --- |
| EF affected common classes on net8/net9/net10 | 1,518 passed; 1 explicit exporter skipped | SQL Server, PostgreSQL, both MySQL drivers, Oracle, SQLite; basic CRUD, schema/default-login cases, querying, facade/change tracking, state application, entity behavior, schema consistency |
| Dapper affected SQLite classes on net8/net9/net10/net472 | 404 passed; 32 unselected remote cases skipped | Common CRUD, handlers, infrastructure, bulk insertion, migration, command options, connection ownership |
| Final Dapper common CRUD, timestamp parameters, handlers, settings and infrastructure on all four targets | 859 passed; zero skipped | 223 on each modern target with six drivers; 190 on net472 with five drivers, excluding unsupported Oracle |
| Production Dapper and both test projects, Release builds | Zero warnings and errors | All configured targets, including production netstandard2.0 and Windows net472 |
| Explicit SQLite schema exporter on net10 | 1 passed | SQL generation without opening a database |

The initial SQLite conversion failure is now a retained regression that passes through production support. No new SQLite unsupported-feature skips or reduced payload assertions were needed. The existing MySQL-only BLOB case now also honors the explicit run selection; selecting both MySQL drivers still runs all its cases.

### Remaining operational work

1. Common cases are designed for parallel execution using independent data. Exclusive operations retain test-level parallelization restrictions; no global CI serialization or resource-slot allocation is introduced.
2. Remote schema fingerprints are not implemented. Provisioning remains explicit, and its refactored path has been compiled but not executed against remote databases.
3. Linux runtime validation remains for CI. Windows validation does not establish Linux SQLite native dependency or filesystem behavior.
4. The production package purpose is unchanged, so the root README and package Description do not require changes. Its package README, design notes, XML documentation, and regressions describe the new conversion behavior.

See [FclEx.DatabaseTesting/README.md](FclEx.DatabaseTesting/README.md) for current operational commands.

## 8. Source References

1. [Dapper project](FclEx.Dapper.Tests/FclEx.Dapper.Tests.csproj) and [EF Core project](FclEx.EfCore.Tests/FclEx.EfCore.Tests.csproj).
2. [Shared framework settings](Directory.Build.props) and [build conditions](../build/General.props).
3. [Dapper fixture](FclEx.Dapper.Tests/FclEx/Dapper/DapperTestsFixture.cs) and [EF fixture](FclEx.EfCore.Tests/FclEx/EfCore/EfCoreFixture.cs).
4. [Target resolution](FclEx.DatabaseTesting/FclEx/Databases/TestDatabaseEnvironment.cs) and [native connection construction](FclEx.DatabaseTesting/FclEx/Databases/TestDatabaseTarget.cs).
5. [Shared entities](FclEx.DatabaseTesting/FclEx/Databases/Entities.cs), [EF model](FclEx.EfCore.Tests/FclEx/EfCore/TestDbContext.cs), and [provisioning](FclEx.EfCore.Tests/FclEx/EfCore/TestDbContextTests.cs).
6. [EF namespace tests](FclEx.EfCore.Tests/FclEx/EfCore/SchemaDbContextTests.cs) and [Dapper common cases](FclEx.Dapper.Tests/FclEx/Dapper/DapperTests.cs).
7. [SQLite migration tests](FclEx.Dapper.Tests/FclEx/Dapper/SqliteMigrationTests.cs) and [specialized EF SQLite lifecycle](FclEx.EfCore.Tests/FclEx/EfCore/Extensions/DbContextExtensions/ApplyChangesSqliteTests.DbContext.cs).
8. [Shared configuration and environment naming](FclEx.Core.Tests/FclEx/CoreTestsFixture.cs) and [CI workflow](../.github/workflows/build.yml).

## 9. Follow-up implementation

Production TableExistsAsync now checks tables by literal name or entity mapping through the built-in provider adapters. Sequence synchronization returns zero for a missing PostgreSQL table before executing setval. Target resolution uses a consistent driver/schema/login argument order, and invalid SQLite namespace scenarios are rejected. The creator-controlled SQLite lifetime and existing common-test parallelization remain unchanged. README driver selection guidance is independent of OS/job allocation. See DATABASE-TESTING-REVIEW.md for the agreed review outcomes; the original requirements and initial assessment above remain historical context.

## 8. Truncate Isolation Update

Truncate result tests retain the common identity, tracking, cancellation, keyless, metadata, mapping, connection-state, and rollback contracts while using isolated physical tables. SQL Server, PostgreSQL, MySQL, and SQLite use connection-local temporary tables. Oracle fixture startup uses `CREATE TABLE IF NOT EXISTS` for four leased table groups per schema, including a parent/child pair with `ON DELETE CASCADE`. Successful closed-connection Dapper coverage and EF default/explicit schema coverage use a separate ordinary table with one test owner per target and project. No truncate class or theory disables parallelization.

Native foreign-key rejection cases have been removed at the repository owner's request. Successful PostgreSQL and Oracle cascade behavior remains covered. SQLite temporary-table sequence reset is a production EF regression fix, with cases both without a main table and with a shadowed main table. The latter verifies that the main table and its sequence remain unchanged.

This design requires a recent Oracle version supporting `CREATE TABLE IF NOT EXISTS`. It does not introduce structural checks or migrations for previously created tables. Exact duplicate runs of one remote matrix entry still require external coordination.

### Validation on Windows

Filtered real-database runs exercised SQL Server, PostgreSQL, MySqlConnector, Oracle, and SQLite. MySql.Data truncate cases remain explicitly skipped.

1. EF Core: 249 applicable truncate/DbSet cases passed on net8, net9, and net10 (net8/net9 include the separately filtered nine ordinary-schema cases). The net10 run with nearby entity and ChangeTracker cases passed 359 tests in about 33 seconds.
2. Dapper: the net8 run with session-isolation and environment tests passed 142 cases; net9 with session-isolation passed 115; net472 with session-isolation passed 91, excluding Oracle as required by that target. The net10 run with neighboring connection/environment cases passed 320 and skipped two unselected MySql.Data cases.
3. The original net10 truncate-only selection passed 106 Dapper cases in about 28 seconds and 240 EF cases in about 28 seconds, before adding the ordinary-schema and session-isolation regressions. These timings describe the observed filtered runs rather than a controlled before/after benchmark.
4. Separate SQLite-only runs passed the temporary identity and main-table isolation regressions. Restricted theories return explicit skipped rows when their drivers are unselected, avoiding xUnit 4.0.1's failed summary for a deferred zero-row theory.

These are focused class/method selections, not complete project or solution runs. Linux execution was not performed locally.
