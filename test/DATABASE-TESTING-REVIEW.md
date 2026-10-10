# Database Testing Review Outcomes

Updated on 2026-10-07 after discussing the review with the repository owner. Existing assertions and common provider cases are retained.

## 1. Default Dapper handlers and type maps

Keep the existing initialization policy. Dapper's parameter type maps can take precedence over a type handler, so installing the default GUID or DateTimeOffset handler requires removing the corresponding maps. This is necessary registration behavior rather than a reason to redesign initialization. Application handlers remain preserved independently; applications that register them are responsible for removing any conflicting parameter maps. The package README and XML documentation now describe this explicitly.

## 2. Missing tables during sequence synchronization

Added production DbConnection.TableExistsAsync overloads for a table name and entity mapping. Provider adapters query their own catalogs using parameterized names. The operation supports CommandOptions, cancellation, transactions, mapping-source overrides, and the established connection ownership rules.

ReseedIdentityAsync is now a production FclEx.Dapper extension supporting every built-in provider and mapped integer identity columns. It returns false when the physical table or identity generator is absent. An unavailable database or invalid connection is still an access error, not a missing-table result. Remote provisioning remains manually enabled and no remote databases or users were rebuilt during validation.

The common tests cover existing and missing tables, mapped names and schemas, literal identifier characters, view exclusion, default and explicit namespaces, and connection state. Additional tests cover transaction visibility and the missing-PostgreSQL-table regression.

## 3. Parallel execution

Keep the current parallel execution design. Common cases use independent rows and are intended to run concurrently in an OS/framework/assembly/schema environment. Tests that require exclusive operations can explicitly disable their parallel execution. No global CI serialization is introduced. Truncate tests now use connection-local temporary tables, except for fixture-precreated Oracle table groups with per-target leases and dedicated ordinary tables for closed-connection coverage. These leases only coordinate the tests owning those tables.

The earlier observation about two separate processes sharing one resource describes a different boundary; it does not establish that the current common cases are incorrect. A test's parallelization setting is process-local. Investigate a concrete conflicting operation before adding cross-process serialization.

## 4. SQLite lifetime

Keep creator-controlled lifetime. Fixtures await initialization, retain their environment for the fixture lifetime, and dispose connections before disposing it. GUID-based directories isolate environments, and successful disposal is idempotent. Additional ready/disposed state management is not required for the current ownership model and was not added.

## 5. Target resolution APIs

Resolve, ResolveTarget, CreateDbConnection, and CreateDbContext now use the consistent argument order driver, schema, login. Default-schema-user callers use a named login argument. The resolver rejects invalid login enum values and explicit server schemas for SQLite. The intentional null-schema/default-login SQLite common scenario remains valid.

## 6. Exact SQLite schema comparison

The exact comparison is in FclEx.EfCore.Tests/FclEx/EfCore/SqliteSchemaTests.cs, method GeneratedSchema_MatchesSharedResource. On every EF target, it generates SQL and compares it with the same embedded baseline; Normalize only handles line endings and trailing whitespace. ExportSchema is the net10-only baseline exporter.

Current provider versions generate matching SQL. No assertions were changed. If a later provider upgrade emits structurally equivalent but textually different SQL, evaluate textual baseline comparison on the exporter target and structural validation on other targets then.

## 7. CI provider coverage documentation

Removed the README's OS-to-driver allocation table. Every supported driver is eligible for CI coverage on each supported operating system, subject to framework/provider compatibility. Driver distribution is an execution-time choice and does not belong in the stable README contract. FCLEX_TEST_DATABASES remains available for focused selection; this update does not change the configured default selection or CI workflow.

## API integration

ISqlAdapter now declares BuildTableExistsCommandText. Direct implementations must implement the member. SqlAdapterBase supplies an unsupported-operation default, so existing derived adapters continue to compile and can opt into metadata querying. Built-in adapters implement the operation. Provider behavior and the scope of catalog visibility are documented in FclEx.Dapper/README.md and public XML documentation.

## Validation

1. Dapper metadata, missing-sequence-table, and target-resolution filters: 280 passed with no skips across net8/net9/net10/net472. Modern targets selected all six drivers; net472 selected the five supported drivers.
2. EF schema/default-login and SQLite schema consistency filters: 54 passed across net8/net9/net10; the explicit schema exporter was the only skipped case.
3. Production FclEx.Dapper built across all configured targets with zero warnings and errors. Public XML documentation, package/root descriptions, README guidance, and the review outcomes were updated together.
4. Validation ran on Windows using real provisioned remote providers and local SQLite. Unique test views were created and removed; remote databases, users, and persistent table structures were not rebuilt. No full solution suite or Linux runtime run was performed.
