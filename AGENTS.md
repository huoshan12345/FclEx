# Repository Guidelines

## Project Structure & Module Organization

FclEx means "fundamental class libraries extensions". It is a multi-package .NET library solution defined by `FclEx.slnx`. It started as extensions for the .NET standard libraries and now also includes focused integration packages for common libraries such as `Microsoft.Extensions.*`, ASP.NET Core, Entity Framework Core, Dapper, Serilog, SlackNet, RabbitMQ, Kafka, Newtonsoft.Json, YamlDotNet, New Relic, and xUnit. Production projects live in `src/`, usually one package per directory such as `src/FclEx.Core`, `src/FclEx.Http`, and `src/FclEx.Serilog`. Tests live in matching `test/*.Tests` projects, with test files mirroring the source namespace layout. Shared MSBuild settings are in `src/Directory.Build.*` and `test/Directory.Build.*`; common build and packaging scripts are in `build/`. Benchmarks and non-shipping experiments belong under `misc/`.

Some source folders intentionally use a `~` prefix, such as `~Extensions`, and have ReSharper namespace provider disabled. This keeps public namespaces flatter and reduces the number of `using` directives needed by package consumers. Test folders should not use the `~` prefix and do not need namespace-provider changes. Because of this, a test namespace may intentionally differ from the source namespace even when the test path mirrors the source path without `~`; for example, `src/FclEx.Http/FclEx/Http/~Extensions/HttpMessageHandlerExtensions.cs` can be in namespace `FclEx.Http`, while `test/FclEx.Http.Tests/FclEx/Http/Extensions/HttpMessageHandlerExtensionsTests.cs` can be in namespace `FclEx.Http.Extensions`.

## Build, Test, and Development Commands

- `dotnet restore FclEx.slnx -v q`: restore all solution packages using `NuGet.Config`.
- `dotnet build FclEx.slnx -c Release --no-restore`: compile all source and test projects in Release mode.
- `dotnet test /nr:false FclEx.slnx -c Release --no-restore --no-build`: run the test suite after a successful build.
- `dotnet pack FclEx.slnx -v q -c Release --include-symbols -p:SymbolPackageFormat=snupkg -o artifacts/nuget`: create NuGet packages locally.
- `pwsh ./build/pack.ps1 -norestore $true`: run the repository packaging script when publishing-style behavior is needed.

## Coding Style & Naming Conventions

Use C# with nullable reference types enabled and `LangVersion` set to `latest`. Keep the existing 4-space indentation and file-scoped namespaces. Write text files as UTF-8 without BOM. Use the native line ending for the current environment: CRLF on Windows and LF on Unix-like systems such as macOS and Linux. Public types and members use `PascalCase`; locals and parameters use `camelCase`; interfaces keep the `I` prefix. Follow existing partial-file naming patterns such as `AssertEx.Equal.cs` and test grouping names such as `DbContextExtensionsTests.ApplyChanges.cs`. Centralize package versions in `src/Directory.Packages.props` or `test/Directory.Packages.props`.

## Code Review Expectations

Use a top-down review order. First determine whether the reviewed functionality has a sound overall design: the problem it solves, whether it should exist in this package, its responsibility boundaries, data and concurrency model, lifecycle, extensibility, and relationship to neighboring types. Only when that overall design is reasonable should the review proceed to detailed API and implementation concerns such as naming, signatures, validation, algorithms, locking, allocation, and code style. If the overall design is unsound, make that the primary finding and propose the appropriate redesign or removal instead of spending the review polishing details that the redesign would discard.

Whenever the user asks for a code review, treat API and signature design as a first-class part of the review, not as optional polish. Review class, method, parameter, local-variable, and public API names for functional accuracy, natural English, readability, and consistency. Also review complete signatures, including overload shape, parameter order and types, return types, generic constraints, nullability, cancellation support, sync/async form, and whether the exposed API accurately expresses its behavior. Call out questionable names or signatures even when the implementation itself is correct.

Review the purpose of each relevant class and method as well as its implementation. If an abstraction exists to solve a recognizable problem but its current responsibility, usage model, or overall design is clearly unsuitable, report that design problem and propose a more appropriate direction. If a type or member's intended purpose is unclear or appears arbitrary, ask the user what problem it is meant to solve before deciding whether its design is reasonable; do not invent an intent merely to justify the existing code.

When the user addresses a batch of review findings, verify every item that the user says they corrected. During that verification, add sufficient regression tests in the matching test project and directory unless the test is demonstrably inapplicable. For every item that passes verification, complete or correct the XML documentation for the affected public API as needed; do not treat implementation verification as complete while its consumer-facing documentation is inaccurate or incomplete.

## Package Boundaries & Public API

Respect the multi-package structure. Keep `FclEx.Core` focused on fundamental, broadly reusable helpers and avoid introducing dependencies on optional external ecosystems there. Put integrations with ASP.NET Core, EF Core, Dapper, Serilog, Slack, messaging, JSON libraries, YAML libraries, or test frameworks in the matching package.

These projects produce public NuGet packages, but design correctness, conceptual coherence, and a clear long-term API take priority over preserving backward compatibility. A change being breaking is not, by itself, a reason to keep a flawed design or suppress a review finding. At minimum, state the design that would be preferable and identify its compatibility impact; whether to carry out a breaking change can then be discussed with the user. When a breaking redesign is authorized, avoid gratuitous churn, but freely rename, remove, or reshape APIs when that produces the more appropriate design, and update tests and documentation together. Always call out public API concerns when you see them, especially unclear names, unnatural English, confusing parameter names, or APIs whose shape does not match their behavior.

When changing package purpose, public surface area, naming, or behavior, check whether the root `README.md`, the package-level `README.md`, and the project `Description` metadata should be updated together.

## Documentation & Work Scope

When writing XML documentation, do not settle for surface-level summaries such as "Gets or sets X" or "Executes the X operation" unless that is genuinely the most useful description. Read the implementation and describe the behavior, important parameters, return values, side effects, failure cases, and compatibility notes that would help a package consumer. If the behavior or intent is unclear after reading the code, ask the user instead of guessing.

If a requested change covers too much code to handle thoughtfully in one pass, say so and propose splitting the work into batches. Prefer smaller, well-reviewed slices over broad mechanical edits that create low-value churn.

## Multi-Targeting & Compatibility

Many packages target multiple frameworks, including older targets such as `netstandard2.0` and `net472` as well as current .NET targets. Before using newer BCL or framework APIs, verify they are available on every target for the project. Use existing compatibility helpers, conditional compilation, or target-specific references when needed.

One purpose of this repository is to backfill APIs that are missing from older .NET versions. When an FclEx extension method has the same name as a method later added by .NET, keep the FclEx API name aligned with the official API instead of renaming it to avoid the conflict. Match the official .NET behavior and use conditional compilation so FclEx provides the method only for target frameworks where the BCL does not.

Source generators are part of the package surface for several projects. When changing generators or generated APIs, check the package that consumes the generator as well as the generator project itself.

## Testing Guidelines

Tests use xUnit v3 and should be added beside the matching package under `test/<Package>.Tests`. Name test classes after the subject, for example `YamlHelperTests`, and use clear method names that describe the behavior under test. Prefer `[Theory]` with `MemberData` for combinatorial cases. Some projects disable parallelization; respect existing fixture and collection patterns before changing test execution behavior.

Organize new tests by the class or interface being tested. Split large test files when multiple tested subjects are involved, and cover boundary cases such as nulls, empty collections, duplicate values, failed operations, exceptions, cancellation, and framework-specific behavior when relevant.

Prefer running tests with `--filter` for the changed test classes or methods before running an entire test project, especially for `FclEx.Core.Tests`, which contains tens of thousands of cases. For example, use `dotnet test test/FclEx.Core.Tests/FclEx.Core.Tests.csproj --filter "FullyQualifiedName~SubjectTests"` when practical. Run the broader project or solution only after the focused tests pass and when the change's scope or risk justifies it; avoid repeatedly running the full Core suite during iteration. Some tests have external service dependencies: `FclEx.Dapper.Tests`, `FclEx.EfCore.Tests`, and `FclEx.Messaging.Tests`. The local test databases and message queues are expected to be provisioned, but be mindful that `FclEx.Messaging` includes Kafka support while `FclEx.Messaging.Tests` currently does not cover Kafka and no Kafka test service is assumed yet.

When a new or changed test fails, do not change the implementation or weaken the test merely to make the suite pass. First determine whether the failure indicates a real implementation issue, an incorrect test assumption, an environment problem, or a legitimately wrong test case. Only modify the implementation or the test when that diagnosis supports it; otherwise, summarize the failure and ask the user how they want to proceed.

When writing a test to reproduce or diagnose a real defect, keep that test in the suite even when it currently fails. A failing reproducer is debugging and regression material; do not delete, skip, weaken, or invert it merely to restore a green test run. Remove or change it only when the user explicitly requests that, or when diagnosis proves that the test expectation itself is incorrect.

When `FclEx.Http` tests need a real local HTTP server, use or follow `test/FclEx.Http.Tests/FclEx/Http/HttpServerFixture.cs`.

## Review and Issue Lists

When listing problems, review findings, or improvement suggestions, prefer numbered lists instead of unordered bullet lists so later discussion can reference items by number. Use multi-level numbering when grouping is necessary.

## File Edits & Encoding

After editing text files, especially with patch tools, verify that the result follows the repository encoding and line-ending rules: UTF-8 without BOM and native line endings for the current OS. On Windows, make sure patched files do not accidentally contain mixed or bare LF line endings.

## Commit & Pull Request Guidelines

Recent history uses short, imperative, lowercase commit subjects such as `refine build props`, `update deps`, and `fix build.yml`. Keep commits focused and avoid mixing source, package, and workflow changes unless required. Pull requests should describe the change, list affected packages, link related issues, and include test results from `dotnet test`. Include screenshots only for documentation or report-output changes.

## Security & Configuration Tips

Do not commit decrypted secrets or local environment files. CI decrypts protected settings through `build/decrypt.ps1` using `SOPS_AGE_KEY`; local changes should preserve that workflow. NuGet publishing expects API keys from environment variables, not checked-in configuration.
