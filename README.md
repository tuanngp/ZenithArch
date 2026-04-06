# RynorArch

**RynorArch** is a compile-time .NET architecture automation framework using Roslyn Incremental Source Generators. It is designed to eliminate boilerplate code for modern .NET applications while enforcing strict clean architecture patterns (CQRS, Repository, or FullStack) with zero runtime overhead and Native AOT compatibility.

## Features

- **Roslyn Incremental Source Generator**: Extremely fast, highly cached, only rebuilds what's necessary.
- **Zero Runtime Reflection**: Generated code operates directly on your types.
- **Deterministic Output**: Consistent, predictable output files (`.g.cs`).
- **Explicit Configuration Required**: Generation fails fast when `[assembly: Architecture(...)]` is missing.
- **Clean Architecture Enforced**: Strict separation of concerns depending on the chosen pattern. Handlers are decoupled from repositories unless FullStack is explicitly used.
- **Hybrid Generation Model**: Public contracts stay explicit per entity, while shared CRUD and EF interaction now flow through a generated generic infrastructure layer emitted once per compilation.
- **Optimized Shared Runtime**: Generic CRUD helpers now cache entity traits, emit one-per-compilation support artifacts, and centralize specification/list semantics to reduce build and runtime overhead.
- **Configurable CQRS Persistence**: Choose per-handler saves or per-request transaction save behavior.
- **Configurable DbContext Type**: CQRS handlers can target a specific DbContext type via `DbContextType = typeof(...)`.
- **Extensible via Partial Classes**: All generated handlers, repositories, and validators are `partial` and provide lifecycle hooks (`OnBeforeHandle`, `OnValidate`, etc.) for you to inject custom logic.
- **DDD Integration**: First-class support for `[AggregateRoot]` and Domain Events.

## Installation

Add references to the core packages in your `.csproj`:

```xml
<!-- The public API package (contains attributes, interfaces) -->
<PackageReference Include="RynorArch.Abstractions" Version="1.0.6" />

<!-- The source generator package (development dependency only) -->
<PackageReference Include="RynorArch.Generator" Version="1.0.6" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

> Replace `1.0.6` with the latest stable package version you intend to adopt, then pin it until you have validated generated output in your project.

### Feature dependencies

| Feature | Required package |
| --- | --- |
| CQRS / FullStack | `MediatR` |
| Validation | `FluentValidation` |
| Persistence | `Microsoft.EntityFrameworkCore` |
| Endpoints | ASP.NET Core shared framework |
| Caching decorators | `Microsoft.Extensions.Caching.*` |

> **Note**: CQRS mode relies on MediatR and FluentValidation (if enabled). Ensure you include these dependencies in your project as well.
```xml
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="FluentValidation" Version="11.11.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
```

## Quick Start

1. Add `RynorArch.Abstractions` and `RynorArch.Generator` to your application project.
2. Add an explicit `[assembly: Architecture(...)]` configuration.
3. Mark your domain classes with `[Entity]` and make them `partial`.
4. Build the project and inspect the generated `.g.cs` files.
5. Extend generated handlers or validators with partial classes only where your business logic diverges.

For a working end-to-end sample, see `samples/RynorArch.Sample`.

## Hybrid Architecture

RynorArch now uses a hybrid model to keep generated source smaller as your entity count grows:

- Per-entity generated types remain explicit for commands, queries, handlers, DTOs, validators, specifications, and domain events.
- Shared CRUD and EF Core interaction are emitted once into a generic infrastructure layer and reused by the per-entity wrappers.
- Generated repositories now act as thin wrappers over a shared `CrudRepository<TEntity>` base instead of duplicating full CRUD implementations for every entity.
- The shared runtime now caches soft-delete traits per `TEntity`, emits `IUnitOfWork` once per compilation, and keeps CQRS/specification filter rules aligned through shared generator helpers.

## Compatibility Matrix

| Area | Status |
| --- | --- |
| Validated SDK | `.NET SDK 10.0.x` |
| Generator package target | `netstandard2.0` |
| CLI runtime targets | `net8.0`, `net9.0`, `net10.0` |
| Supported patterns | `Cqrs`, `Repository`, `FullStack` |

See `docs/COMPATIBILITY.md` for the detailed support matrix and adoption guidance.

## Configuration

RynorArch uses an assembly-level configuration attribute to determine its behavior. Place this in any `.cs` file in your main project (e.g., `AssemblyConfig.cs`):

```csharp
using RynorArch.Abstractions.Attributes;
using RynorArch.Abstractions.Enums;

[assembly: Architecture(
    Pattern = ArchitecturePattern.Cqrs,
    UseSpecification = true,
    UseUnitOfWork = false,
    EnableValidation = true,
    DbContextType = typeof(MyApp.Infrastructure.Data.AppDbContext),
    CqrsSaveMode = CqrsSaveMode.PerRequestTransaction
)]
```

### Supported Patterns

- `ArchitecturePattern.Cqrs` - Generates MediatR Commands, Queries, and Handlers. Handlers dependency-inject either `DbContext` or the type configured by `DbContextType`.
- `ArchitecturePattern.Repository` - Generates repository interfaces and thin repository wrappers backed by shared generic CRUD infrastructure, plus an optional `UnitOfWork` interface.
- `ArchitecturePattern.FullStack` - Generates both CQRS and Repository artifacts on top of the shared CRUD/runtime layer.

Always declare the assembly-level configuration explicitly, even if the defaults happen to match your current needs. This keeps upgrades and generated output predictable.

### Endpoint generation policy

Endpoint generation is intentionally gated as experimental. To enable it, both flags are required:

```csharp
[assembly: Architecture(
    Pattern = ArchitecturePattern.Cqrs,
    GenerateEndpoints = true,
    EnableExperimentalEndpoints = true
)]
```

### CQRS save modes

- `CqrsSaveMode.PerHandler` (default): each write handler calls `SaveChangesAsync` immediately.
- `CqrsSaveMode.PerRequestTransaction`: generated write handlers stage changes and a generated MediatR pipeline behavior commits once per request in a transaction.

## Usage

Define a domain entity and decorate it with `[Entity]`:

```csharp
using RynorArch.Abstractions.Attributes;
using RynorArch.Abstractions.Base;

namespace MyApp.Domain;

[Entity]
[AggregateRoot] // Optional: Generates Domain Event records
public partial class Trip : EntityBase
{
    [QueryFilter]
    public required string Destination { get; set; }
    
    public required DateTime StartDate { get; set; }
    
    // ...other properties
}
```

### What gets generated?

If set to **CQRS** mode with Validation and Specifications enabled, the generator will produce:

1. **Commands & Queries**: `CreateTripCommand`, `UpdateTripCommand`, `DeleteTripCommand`, `GetTripByIdQuery`, `GetTripListQuery`.
2. **Handlers**: `CreateTripHandler`, `UpdateTripHandler`, etc., which now delegate shared persistence concerns into generated generic infrastructure.
3. **Specifications**: `TripSpecification` mapped to `[QueryFilter]` properties.
4. **Validators**: `CreateTripValidator`, `UpdateTripValidator` stubs with basic rules implemented.
5. **Domain Events**: `TripCreatedEvent`, `TripUpdatedEvent`, `TripDeletedEvent` (because of `[AggregateRoot]`).
6. **Shared Infrastructure**: one generated CRUD/runtime support file reused across all entities in the compilation.

When caching decorators are enabled, generation also includes cache invalidation contracts per entity and default distributed-cache invalidator implementations.

### Extending Generated Code

Because all handlers are partial, you can easily provide your own validation or complex business logic via the partial method hooks:

```csharp
namespace MyApp.Cqrs;

public partial class CreateTripHandler
{
    partial void OnValidate(CreateTripCommand command)
    {
        // Custom synchronous validation check before processing
    }

    partial void OnBeforeHandle(CreateTripCommand command, Trip entity)
    {
        // Mutate the entity before it saves to the database
        entity.StartDate = entity.StartDate.ToUniversalTime();
    }
}
```

Similarly, validators can be extended:

```csharp
namespace MyApp.Validation;

public partial class CreateTripValidator 
{
    partial void ConfigureRules()
    {
        RuleFor(x => x.Destination).MinimumLength(3);
    }
}
```

## Samples and Generated Files

- `samples/RynorArch.Sample` demonstrates a realistic consumer project with `FullStack` mode enabled.
- Generated files should usually stay out of source control unless your team intentionally reviews generated diffs as part of the release process.
- If you were depending on the old fully generated repository implementation shape, read `docs/UPGRADING.md` before updating.
- A global `RynorArch.GenerationReport.g.cs` file is emitted to summarize entities, feature flags, and generated artifact names.
- Generated files now include traceability metadata comments (`rynor-artifact`, `rynor-entity`, `rynor-assumptions`) to speed up debugging.

## Troubleshooting and Upgrades

- Troubleshooting: `docs/TROUBLESHOOTING.md`
- Compatibility: `docs/COMPATIBILITY.md`
- Upgrade guidance: `docs/UPGRADING.md`
- Release process: `docs/RELEASING.md`
- Changelog: `CHANGELOG.md`

## Built With

- **.NET 10 SDK**
- **Roslyn Incremental Source Generators** API (`IIncrementalGenerator`)
- **MediatR**
- **FluentValidation**
- **Entity Framework Core**
