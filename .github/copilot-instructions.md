# Project Guidelines

## Scope
RynorArch is a compile-time architecture automation framework powered by Roslyn incremental source generation. Prefer deterministic output, explicit diagnostics, and minimal hidden runtime behavior.

## Code Style
- Honor shared defaults in Directory.Build.props (LangVersion 13, nullable enabled, implicit usings enabled, warnings as errors).
- Keep generated output deterministic (stable ordering, no hidden runtime side effects).
- Treat diagnostics as part of the public developer experience: fail-fast and actionable.

## Architecture
Primary component boundaries:
- src/RynorArch.Abstractions: public attributes, interfaces, base contracts.
- src/RynorArch.Generator: Roslyn incremental generator and emitters.
- src/RynorArch.Cli: init/scaffold/doctor workflows and global tool packaging.
- samples/RynorArch.Sample: canonical wiring for DI, DbContext, and endpoint mapping.
- tests/RynorArch.Generator.Tests: generator output and diagnostics checks.
- tests/RynorArch.Integration.Tests: runtime semantics (CRUD, soft delete, audit, caching, validation, transactions).
- tests/RynorArch.E2E.Tests: CLI smoke and endpoint semantics.

For generator changes:
- Use attribute-indexed discovery (ForAttributeWithMetadataName), not broad compilation scans.
- Do not hand-edit generated files under obj/; change source models, attributes, or emitters instead.

## Build and Test
Use .NET SDK 10.0.x.

Core commands:
- dotnet restore RynorArch.slnx
- dotnet build RynorArch.slnx --configuration Release --no-restore
- dotnet test RynorArch.slnx --configuration Release --no-build

Targeted tests:
- dotnet test tests/RynorArch.Generator.Tests/RynorArch.Generator.Tests.csproj
- dotnet test tests/RynorArch.Integration.Tests/RynorArch.Integration.Tests.csproj
- dotnet test tests/RynorArch.E2E.Tests/RynorArch.E2E.Tests.csproj --configuration Release

NuGet packaging:
- pwsh ./publish.ps1 -Increment None
- pwsh ./publish.ps1 -Increment Patch
- pwsh ./publish.ps1 -Increment Minor -Push

Workflow gate for architecture or generator changes:
1. dotnet build RynorArch.slnx --configuration Release
2. dotnet run --project src/RynorArch.Cli/RynorArch.Cli.csproj -- doctor "<project-path>"
3. Continue only when doctor reports READY or READY WITH WARNINGS.

## NuGet Package Conventions
- Versioning is centralized in Directory.Build.props via VersionPrefix; do not hardcode per-project package versions.
- Prefer publish.ps1 for bump/build/test/pack/push so version updates and artifacts stay consistent.
- Packaged projects are src/RynorArch.Abstractions, src/RynorArch.Generator, and src/RynorArch.Cli.
- Keep package metadata coherent with release intent (README, release notes, tags, repository URLs).

## Project Conventions
- Classes marked with [Entity] must be partial.
- Endpoint generation requires both GenerateEndpoints = true and EnableExperimentalEndpoints = true.
- If Architecture.DbContextType is omitted and generated handlers depend on DbContext, register:
  services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>())
- When public behavior changes, update tests plus user-facing docs and changelog entries.

## Link, Do Not Embed
Use these as source-of-truth references instead of duplicating long guidance:
- docs/AI_AGENT_PLAYBOOK.en.md: AI workflow contracts and readiness gates.
- docs/GETTING_STARTED.en.md: onboarding and first integration path.
- docs/ATTRIBUTE_REFERENCE.en.md: attribute-level contract details.
- docs/INTEGRATION_GUIDE.en.md: runtime DI wiring and endpoint mapping.
- docs/ENDPOINT_HARDENING.en.md: production endpoint hardening checklist.
- docs/RUNTIME_TESTING.en.md: runtime and integration testing strategy.
- docs/TROUBLESHOOTING.en.md: common failure modes and fixes.
- docs/RELEASING.en.md: release and NuGet publishing process.
- CONTRIBUTING.md: contributor expectations and quality bar.
- CHANGELOG.md and docs/UPGRADING.en.md: release notes and migration guidance.
# Project Guidelines

## Scope
RynorArch is a compile-time architecture automation framework powered by Roslyn incremental source generation. Prefer deterministic output, explicit diagnostics, and minimal hidden runtime behavior.

## Build and Test
Use .NET SDK 10.0.x.

Core commands:
- dotnet restore RynorArch.slnx
- dotnet build RynorArch.slnx --configuration Release --no-restore
- dotnet test RynorArch.slnx --configuration Release --no-build

Targeted test commands:
- dotnet test tests/RynorArch.Generator.Tests/RynorArch.Generator.Tests.csproj
- dotnet test tests/RynorArch.Integration.Tests/RynorArch.Integration.Tests.csproj
- dotnet test tests/RynorArch.E2E.Tests/RynorArch.E2E.Tests.csproj --configuration Release

Packaging:
- pwsh ./publish.ps1 -Increment None

When implementing workflow changes, use this gate:
1. dotnet build
2. dotnet run --project src/RynorArch.Cli/RynorArch.Cli.csproj -- doctor "<project-path>"
3. Continue only when doctor reports READY or READY WITH WARNINGS.

## Architecture
Primary component boundaries:
- src/RynorArch.Abstractions: public attributes, interfaces, base contracts.
- src/RynorArch.Generator: Roslyn incremental generator and emitters.
- src/RynorArch.Cli: init/scaffold/doctor workflows.
- samples/RynorArch.Sample: canonical wiring for DI, DbContext, and endpoint mapping.
- tests/RynorArch.Generator.Tests: generator output and diagnostics checks.
- tests/RynorArch.Integration.Tests: runtime semantics (CRUD, soft delete, audit, caching, validation, transactions).
- tests/RynorArch.E2E.Tests: CLI smoke tests.

For generator changes, preserve incremental design:
- Use attribute-indexed discovery (ForAttributeWithMetadataName), not broad compilation scans.
- Keep generator outputs deterministic.
- Keep diagnostics actionable and fail-fast.

## Conventions
- Classes marked with [Entity] must be partial.
- Do not hand-edit generated files in obj/; change source models, attributes, or emitters instead.
- If Architecture.DbContextType is omitted and handlers depend on DbContext, register:
  services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>())
- Endpoint generation requires both GenerateEndpoints = true and EnableExperimentalEndpoints = true.
- Treat warnings as errors and keep code style/build checks clean.
- When public behavior changes, update docs under docs/ and changelog entries as needed.

## Link, Do Not Embed
Use existing docs as source of truth instead of repeating their content:
- docs/AI_AGENT_PLAYBOOK.md: AI workflow contracts and readiness gates.
- docs/GETTING_STARTED.en.md: onboarding and first integration path.
- docs/ATTRIBUTE_REFERENCE.en.md: attribute-level contract details.
- docs/INTEGRATION_GUIDE.en.md: runtime DI wiring and endpoint mapping.
- docs/ENDPOINT_HARDENING.en.md: production endpoint hardening checklist.
- docs/TROUBLESHOOTING.en.md: common failure modes and fixes.
- CONTRIBUTING.md: contributor expectations and quality bar.
