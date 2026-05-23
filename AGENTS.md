# Repository Guidelines

## Project Structure & Module Organization

FclEx is a multi-package .NET library solution defined by `FclEx.slnx`. Production projects live in `src/`, usually one package per directory such as `src/FclEx.Core`, `src/FclEx.Http`, and `src/FclEx.Serilog`. Tests live in matching `test/*.Tests` projects, with test files mirroring the source namespace layout. Shared MSBuild settings are in `src/Directory.Build.*` and `test/Directory.Build.*`; common build and packaging scripts are in `build/`. Benchmarks and non-shipping experiments belong under `misc/`.

## Build, Test, and Development Commands

- `dotnet restore FclEx.slnx -v q`: restore all solution packages using `NuGet.Config`.
- `dotnet build FclEx.slnx -c Release --no-restore`: compile all source and test projects in Release mode.
- `dotnet test /nr:false FclEx.slnx -c Release --no-restore --no-build`: run the test suite after a successful build.
- `dotnet pack FclEx.slnx -v q -c Release --include-symbols -p:SymbolPackageFormat=snupkg -o artifacts/nuget`: create NuGet packages locally.
- `pwsh ./build/pack.ps1 -norestore $true`: run the repository packaging script when publishing-style behavior is needed.

## Coding Style & Naming Conventions

Use C# with nullable reference types enabled and `LangVersion` set to `latest`. Keep the existing 4-space indentation and file-scoped namespaces. Public types and members use `PascalCase`; locals and parameters use `camelCase`; interfaces keep the `I` prefix. Follow existing partial-file naming patterns such as `AssertEx.Equal.cs` and test grouping names such as `DbContextExtensionsTests.ApplyChanges.cs`. Centralize package versions in `src/Directory.Packages.props` or `test/Directory.Packages.props`.

## Testing Guidelines

Tests use xUnit v3 and should be added beside the matching package under `test/<Package>.Tests`. Name test classes after the subject, for example `YamlHelperTests`, and use clear method names that describe the behavior under test. Prefer `[Theory]` with `MemberData` for combinatorial cases. Some projects disable parallelization; respect existing fixture and collection patterns before changing test execution behavior.

## Commit & Pull Request Guidelines

Recent history uses short, imperative, lowercase commit subjects such as `refine build props`, `update deps`, and `fix build.yml`. Keep commits focused and avoid mixing source, package, and workflow changes unless required. Pull requests should describe the change, list affected packages, link related issues, and include test results from `dotnet test`. Include screenshots only for documentation or report-output changes.

## Security & Configuration Tips

Do not commit decrypted secrets or local environment files. CI decrypts protected settings through `build/decrypt.ps1` using `SOPS_AGE_KEY`; local changes should preserve that workflow. NuGet publishing expects API keys from environment variables, not checked-in configuration.
