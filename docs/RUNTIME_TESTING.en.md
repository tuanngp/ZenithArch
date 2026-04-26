# Runtime Testing Guide

[Tiếng Việt](RUNTIME_TESTING.md) | [English](RUNTIME_TESTING.en.md)


This guide validates generated runtime behavior end-to-end, not just generated source shape.

## Why this exists

`ZenithArch.Generator.Tests` validates diagnostics and generated output contracts.
`ZenithArch.Integration.Tests` validates runtime behavior against a relational provider.

Both are required before release.

## Test project

Runtime integration tests live in:

- `tests/ZenithArch.Integration.Tests`

The test host uses:

- SQLite in-memory (`Microsoft.EntityFrameworkCore.Sqlite`)
- Generated CQRS handlers and MediatR pipeline behaviors from `samples/ZenithArch.Sample`
- Generated caching invalidators and validation behavior

## Covered scenarios

The runtime suite currently verifies:

- CRUD roundtrip through generated commands and handlers
- Soft-delete query exclusion (`ISoftDelete`)
- Audit timestamp stamping (`IAuditable`)
- Validation failure prevents persistence
- Per-request transaction rollback on handler failure
- Cache population and invalidation for `GetById` query pipeline

## Run locally

```powershell
dotnet test tests/ZenithArch.Integration.Tests/ZenithArch.Integration.Tests.csproj -c Release
```

To run the full release gate:

```powershell
dotnet restore ZenithArch.slnx
dotnet build ZenithArch.slnx -c Release
dotnet test ZenithArch.slnx -c Release
dotnet run --project src/ZenithArch.Cli/ZenithArch.Cli.csproj -- doctor samples/ZenithArch.Sample
```

## Generator performance baseline (compile-time)

Run dry benchmarks for generator hot paths:

```powershell
dotnet run --project tests/ZenithArch.Performance.Tests/ZenithArch.Performance.Tests.csproj -c Release -- --filter *RunGenerator* --job Dry
```

Benchmark artifacts are written to:

- `tests/ZenithArch.Performance.Tests/BenchmarkDotNet.Artifacts/results`

## Writing new runtime tests

1. Reuse `IntegrationTestHost` to boot a SQLite-backed service collection.
2. Send generated commands through `IMediator`.
3. Assert both API-level return semantics and persisted state in `AppDbContext`.
4. Prefer behavior assertions over source-text assertions.

## Notes

- Keep endpoint generation marked experimental unless explicitly promoted.
- If `DbContextType` is not configured, generated handlers depend on `DbContext`; integration host currently maps `DbContext` to `AppDbContext` explicitly for deterministic DI behavior.
