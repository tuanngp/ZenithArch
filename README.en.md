# Zenith Arch

[Vietnamese](https://github.com/tuanngp/ZenithArch/blob/main/README.vi.md) | [English](https://github.com/tuanngp/ZenithArch/blob/main/README.md)

## Overview

Zenith Arch is a compile-time .NET architecture automation framework powered by Roslyn Incremental Source Generators. It removes repetitive boilerplate while enforcing consistent clean architecture boundaries.

## Why Zenith Arch

- Compile-time generation with deterministic `.g.cs` output
- No runtime reflection in the primary execution path
- Explicit assembly-level architecture contract with fail-fast diagnostics
- Hybrid generation model: per-entity contracts + shared runtime infrastructure emitted once per compilation
- Native AOT-friendly design direction

## Installation

```xml
<PackageReference Include="ZenithArch.Abstractions" Version="1.0.7" />
<PackageReference Include="ZenithArch.Generator" Version="1.0.7" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

Optional CLI setup:

```bash
dotnet tool install --global ZenithArch.Cli --version 1.0.7
dotnet tool update --global ZenithArch.Cli --version 1.0.7
```

## Feature Dependencies

| Feature | Required dependency |
| --- | --- |
| CQRS / FullStack handlers | MediatR |
| Validation generation | FluentValidation |
| Persistence | Microsoft.EntityFrameworkCore |
| Endpoint generation | Microsoft.AspNetCore.App |
| Caching decorators | Microsoft.Extensions.Caching.* |

## Quick Start

### Path A (CLI-first)

1. Add package references.
2. Run `rynor init`.
3. Run `rynor scaffold Trip MyApp.Domain`.
4. Run `dotnet build`.
5. Register generated runtime: `builder.Services.AddZenithArchDependencies();`.

### Path B (manual generator-first)

1. Add package references.
2. Create `AssemblyConfig.cs` with `[assembly: Architecture(...)]`.
3. Mark entity classes with `[Entity]` and `partial`.
4. Build and inspect generated output in `obj/`.
5. Register generated runtime in startup.

## Architecture Configuration

```csharp
using ZenithArch.Abstractions.Attributes;
using ZenithArch.Abstractions.Enums;

[assembly: Architecture(
    Profile = ArchitectureProfile.CqrsQuickStart,
    Pattern = ArchitecturePattern.Cqrs,
    GenerateDependencyInjection = true,
    DbContextType = typeof(MyApp.Infrastructure.Data.AppDbContext)
)]
```

### Supported Patterns

- `ArchitecturePattern.Cqrs`
- `ArchitecturePattern.Repository`
- `ArchitecturePattern.FullStack`

### Starter Profiles

- `ArchitectureProfile.CqrsQuickStart`
- `ArchitectureProfile.RepositoryQuickStart`
- `ArchitectureProfile.FullStackQuickStart`

## Complete Supported Feature Catalog

### ArchitectureAttribute flags

- `UseSpecification`
- `UseUnitOfWork`
- `EnableValidation`
- `GenerateDependencyInjection`
- `GenerateEndpoints` + `EnableExperimentalEndpoints`
- `GenerateDtos`
- `GenerateEfConfigurations`
- `GenerateCachingDecorators`
- `GeneratePagination`
- `DbContextType`
- `CqrsSaveMode` (`PerHandler` or `PerRequestTransaction`)

### Domain and attribute support

- `[Entity]`
- `[AggregateRoot]`
- `[QueryFilter]`
- `[MapTo(typeof(...))]`
- Validation attributes: `[Required]`, `[MinLength]`, `[MaxLength]`, `[Email]`

### Runtime extensibility support

- Partial handlers/repositories/validators
- Lifecycle hooks (`OnValidate`, `OnBeforeHandle`, `OnAfterHandle`, `OnBeforeQuery`)
- Optional observer integration: `IZenithArchExecutionObserver`
- Optional security context integration: `ISecurityContext`

### Runtime behaviors covered

- CQRS CRUD semantics
- Soft delete (`ISoftDelete`)
- Audit stamping (`IAuditable`)
- Validation pipeline behavior
- Per-request transaction save mode
- Query caching + invalidation
- Experimental endpoint generation with defined write semantics

### CLI support

- `rynor init`
- `rynor scaffold <EntityName> [Namespace]`
- `rynor doctor [ProjectPath]`

## Compatibility

- Validated SDK: `.NET SDK 10.0.x`
- Generator target: `netstandard2.0`
- CLI runtimes: `net8.0`, `net9.0`, `net10.0`

## Documentation

- [Getting Started](https://github.com/tuanngp/ZenithArch/blob/main/docs/GETTING_STARTED.en.md)
- [Feature Matrix](https://github.com/tuanngp/ZenithArch/blob/main/docs/FEATURE_MATRIX.en.md)
- [Integration Guide](https://github.com/tuanngp/ZenithArch/blob/main/docs/INTEGRATION_GUIDE.en.md)
- [AI Agent Playbook](https://github.com/tuanngp/ZenithArch/blob/main/docs/AI_AGENT_PLAYBOOK.en.md)
- [Attribute Reference](https://github.com/tuanngp/ZenithArch/blob/main/docs/ATTRIBUTE_REFERENCE.en.md)
- [Compatibility](https://github.com/tuanngp/ZenithArch/blob/main/docs/COMPATIBILITY.en.md)
- [Endpoint Hardening](https://github.com/tuanngp/ZenithArch/blob/main/docs/ENDPOINT_HARDENING.en.md)
- [Caching Operations](https://github.com/tuanngp/ZenithArch/blob/main/docs/CACHING_OPERATIONS.en.md)
- [Runtime Testing](https://github.com/tuanngp/ZenithArch/blob/main/docs/RUNTIME_TESTING.en.md)
- [Troubleshooting](https://github.com/tuanngp/ZenithArch/blob/main/docs/TROUBLESHOOTING.en.md)
- [Upgrading](https://github.com/tuanngp/ZenithArch/blob/main/docs/UPGRADING.en.md)
- [Upgrading Profiles](https://github.com/tuanngp/ZenithArch/blob/main/docs/UPGRADING_PROFILES.en.md)
- [Releasing](https://github.com/tuanngp/ZenithArch/blob/main/docs/RELEASING.en.md)
- [Changelog](https://github.com/tuanngp/ZenithArch/blob/main/CHANGELOG.md)
