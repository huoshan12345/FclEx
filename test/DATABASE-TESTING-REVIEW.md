# Database Testing Design Review

Reviewed on 2026-10-07. Behavioral changes below are proposals for discussion. This review applied naming changes and documentation only; it preserved assertions, provider cases, and runtime behavior.

## Overall assessment

The shared ordinary library is an appropriate boundary: both executable projects can run independently, including Dapper on Windows net472, without introducing an EF runtime dependency into shared initialization. Keeping the existing EF model authoritative and embedding its generated SQLite creation script fits disposable tests and pre-provisioned remote databases. A migration-framework replacement or further project split is not justified by this review.

## Findings

### 1. Default handler initialization removes application type maps (P2)

Source: `src/FclEx.Dapper/FclEx/Dapper/DapperHelper.cs`, `Initialize`.

The guard preserves existing type handlers, but `RemoveTypeMap` does not distinguish a built-in mapping from an application's `SqlMapper.AddTypeMap` registration. An application that customizes DateTimeOffset parameter binding before its first FclEx helper call loses that setting as a side effect of initialization. The same mechanism applies to the existing GUID registration.

An isolated, database-free diagnostic registered DateTimeOffset as DbType.String and queried Dapper's LookupDbType before and after initialization. Before: String. After: Object with DateTimeOffsetTypeHandler. This verifies the overwrite; it does not establish that a particular application's SQL command fails.

Recommendation: define whether FclEx owns those process-wide type maps or preserves application mappings. If preserving them, separate result-handler registration from parameter-mapping policy and provide an explicit way to opt into default parameter handling. If overriding is intentional, expose that choice explicitly and document that application type maps are replaced. Add permanent mapping and parameter-binding regressions when implementing the agreed policy.

### 2. Remote setup still depends on normal-run initialization (P2)

Sources: `TestDbContextTests.SetupDatabase`, `EfCoreFixture.InitializeAsync`, and `DatabaseTestSettings`.

The setup class inherits EfCoreTests and therefore requires EfCoreFixture. With PostgreSQL selected, fixture startup executes identity-sequence synchronization against already existing tables. Enabling setup on a missing or partially prepared PostgreSQL environment can fail before setup reaches database creation. Selecting SQLite avoids that particular dependency, but makes a separate environment override a prerequisite for preparing remote resources.

The driver lists also share one static type initializer. A reflection diagnostic confirmed that reading ProvisioningDrivers with FCLEX_TEST_DATABASES=unknown fails because SelectedDrivers parsing runs in the same initializer. The provisioning list's values are independent, but its initialization is still coupled to normal-run configuration.

Setup currently requires editing the hardcoded Skip on LocalOnlyTheory. It has no usable explicit-test entry point comparable to the SQLite exporter. The cartesian resource matrix also includes combinations that normal tests never execute, such as EF on framework 4 and framework 4 on Linux.

Recommendation: keep remote setup explicitly invoked, but give it a configuration-only fixture or a fixture-free class; use an explicit test entry point; isolate provisioning selection from normal-run static initialization; and describe supported resource combinations in one manifest. Do not recreate remote resources during normal test startup. This review did not execute setup or delete remote resources.

### 3. Same-environment runs can still race across processes (P2)

Sources: `.github/workflows/build.yml`, fixture naming via CoreTestsFixture.WithAssemblyInfo, and identity-sequence synchronization.

Project/framework/OS/schema naming isolates those dimensions, but two workflows, or two local runs, with the same combination share remote tables and sequence state. The workflow has no concurrency group. DisableParallelization is process-local and does not serialize another test process. A second run can synchronize a sequence while the first is inserting rows or temporarily using explicit generated keys.

Recommendation: serialize CI runs that use the same pre-provisioned resource set, without cancelling an active database test, or allocate a small pool of pre-provisioned stable slots. Include slot identity in both database and user/schema names. Local overlap needs the same lease mechanism or an explicitly documented restriction. Separate test projects and operating systems can retain their existing parallel execution.

### 4. SQLite lifecycle has no explicit ready/disposed state (P3)

Source: `TestDatabaseEnvironment`.

The presence of _sqliteDirectory represents both initializing and initialized states: it is assigned before schema creation finishes. Resolve can expose that file during initialization. Dispose clears the field, after which InitializeSqliteAsync accepts another initialization; remote resolution is also still allowed after disposal. Initialization failure calls Dispose directly, so a cleanup exception can replace the original error.

Current xUnit fixtures await initialization before exposing connections, so this is lifecycle hardening rather than a reproduced failure of their current execution path.

Recommendation: define uninitialized, initializing, ready, and disposed states; publish the target after initialization succeeds; reject use after disposal; and preserve the original initialization failure when cleanup also fails. Do not add general locking unless concurrent initialization is intentionally supported. Add targeted cancellation, failed-initialization cleanup, repeated-disposal, and use-after-disposal tests with the implementation.

### 5. Resolver signatures and unsupported scenarios need clearer contracts (P3)

Sources: `TestDatabaseEnvironment.Resolve`, `EfCoreFixture.ResolveTarget`, and `TestDatabaseTarget`.

The environment accepts arguments in driver/schema/login order, while the EF wrapper accepts driver/login/schema. Both represent the same resolution operation. Current named arguments keep callers correct, but future wrappers can easily diverge.

SQLite returns early, so a non-null server schema or even an invalid login enum is ignored. The target record can also represent mutually inconsistent combinations of database, schema, file path, and server configuration. The existing null-schema/default-login SQLite common test is intentional and must remain supported.

Recommendation: use one parameter order for target resolution, validate inappropriate explicit schema and identity combinations, and distinguish requested schema qualification from resolved connection identity in documentation. Prefer small validation rules or named factories before introducing a hierarchy of provider-specific target types. Keep all existing legitimate remote and SQLite common cases.

### 6. SQLite schema consistency checks conflate structure and generated SQL text (P3)

Source: `SqliteSchemaTests.GeneratedSchema_MatchesSharedResource`.

One net10-generated baseline is compared byte-for-byte, apart from line endings and trailing whitespace, against every EF target. Current versions generate identical SQL, but a harmless provider upgrade can change statement order or formatting while preserving the schema and supported behavior.

Recommendation: retain exact comparison on the designated exporter target. On other targets, compare the resulting tables, columns, keys, indexes, and foreign keys, and retain the existing common integration tests. This keeps drift detection without requiring EF versions to emit identical SQL. No consistency assertions were removed in this review.

### 7. The documented Linux default selection is stale (P3)

Sources: `DatabaseTestSettings.SelectDrivers` and `FclEx.DatabaseTesting/README.md`.

The current Linux CI branch returns SupportedDrivers, which includes Npgsql and MySql.Data in addition to the earlier remote selection and SQLite. The README table still lists only SqlServer, MySqlConnector, Oracle, and Sqlite and claims that remote defaults were unchanged.

Recommendation: update the documentation to describe the current expanded Linux matrix and add an exact-set assertion for each default selection. The current settings test checks only a representative remote driver, SQLite, and absence of duplicates; it does not protect the complete expected default set. Preserve the expanded coverage unless a different selection is deliberately agreed.

## Naming changes applied

1. DbDrivers -> SelectedDrivers, to distinguish normal-run selection from SupportedDrivers and ProvisioningDrivers.
2. DbName -> DatabaseName; ProviderEnvironmentVariable -> DriverSelectionEnvironmentVariable.
3. FixAutoIncrement -> SynchronizeIdentitySequenceAsync, with documentation of its PostgreSQL-only behavior and persistent sequence side effects.
4. DatabaseTestSettings.SchemaCases -> GetDriverSchemaCases, to describe its driver/schema combinations and avoid confusion with schema-only theory data.
5. Fixture Environment -> DatabaseEnvironment, to make the owned resource clear and avoid collision with System.Environment.
6. Linked assertion class Extensions -> DatabaseAssertExtensions, including its filename.
7. TestDbContext.UseMySql -> ConfigureMySqlConnector; ver -> serverVersion; resolved conStr locals -> target.

## Validation

1. Both test projects built in Release across all configured frameworks: zero warnings and errors.
2. Dapper settings and environment tests: 132 passed across net8/net9/net10/net472, using disposable local SQLite and offline remote-target resolution.
3. EF schema/default-login and SQLite schema tests on net10 with all six drivers selected: 18 passed; only the explicitly invoked schema exporter was skipped.
4. The two process-wide initialization diagnostics were database-free and ran in isolated shell processes. Lifecycle hardening and cross-process race findings are based on the code paths, not a claimed reproduced concurrent failure.

No destructive remote setup was executed, and no behavior proposals in this document were implemented.
