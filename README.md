# Zenith Arch

Compile-time architecture automation for .NET with Roslyn incremental source generation.

[![NuGet ZenithArch.Abstractions](https://img.shields.io/nuget/v/ZenithArch.Abstractions.svg)](https://www.nuget.org/packages/ZenithArch.Abstractions)
[![NuGet ZenithArch.Generator](https://img.shields.io/nuget/v/ZenithArch.Generator.svg)](https://www.nuget.org/packages/ZenithArch.Generator)
[![NuGet ZenithArch.Cli](https://img.shields.io/nuget/v/ZenithArch.Cli.svg)](https://www.nuget.org/packages/ZenithArch.Cli)

## Installation

```bash
dotnet add package ZenithArch.Abstractions
dotnet add package ZenithArch.Generator
```

For source generator usage, configure analyzer-style reference:

```xml
<ItemGroup>
  <PackageReference Include="ZenithArch.Abstractions" Version="1.0.8" />
  <PackageReference Include="ZenithArch.Generator" Version="1.0.8" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
</ItemGroup>
```

Optional CLI installation:

```bash
dotnet tool install --global ZenithArch.Cli --version 1.0.8
```

## Quickstart

```csharp
using ZenithArch.Abstractions.Attributes;
using ZenithArch.Abstractions.Enums;

[assembly: Architecture(
    Profile = ArchitectureProfile.CqrsQuickStart,
    Pattern = ArchitecturePattern.Cqrs,
    GenerateDependencyInjection = true,
    DbContextType = typeof(AppDbContext)
)]

namespace Demo.Domain;

[Entity]
public partial class Trip
{
    public Guid Id { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;
}
```

Build once:

```bash
dotnet build
```

Then register generated DI extensions:

```csharp
builder.Services.AddZenithArchDependencies();
```

## Full API Reference

### ZenithArch.Abstractions.Attributes

- `ArchitectureAttribute`: Assembly-level generation contract (pattern, profile, feature flags).
  - Example: `[assembly: Architecture(Pattern = ArchitecturePattern.Cqrs, GenerateDependencyInjection = true)]`
- `EntityAttribute`: Marks a class as a generation candidate entity.
  - Example: `[Entity] public partial class Trip { }`
- `AggregateRootAttribute`: Marks an entity as an aggregate root for domain-event-enabled generation.
  - Example: `[Entity, AggregateRoot] public partial class Order : EntityBase { }`
- `MapToAttribute`: Declares DTO/view model mapping targets.
  - Example: `[MapTo(typeof(TripDto))] public partial class Trip : EntityBase { }`
- `QueryFilterAttribute`: Marks properties as generated filter criteria.
  - Example: `[QueryFilter] public string? Name { get; set; }`
- `RequiredAttribute`: Marks a property as required for generated validators.
  - Example: `[Required] public string Name { get; set; } = string.Empty;`
- `MinLengthAttribute`: Declares minimum length validation.
  - Example: `[MinLength(3)] public string Name { get; set; } = string.Empty;`
- `MaxLengthAttribute`: Declares maximum length validation.
  - Example: `[MaxLength(120)] public string Name { get; set; } = string.Empty;`
- `EmailAttribute`: Marks a property for email-format validation.
  - Example: `[Email] public string ContactEmail { get; set; } = string.Empty;`

### ZenithArch.Abstractions.Enums

- `ArchitecturePattern`: Selects CQRS, Repository, or FullStack generation topology.
  - Example: `Pattern = ArchitecturePattern.FullStack`
- `ArchitectureProfile`: Applies starter flag presets.
  - Example: `Profile = ArchitectureProfile.RepositoryQuickStart`
- `CqrsSaveMode`: Controls write persistence timing.
  - Example: `CqrsSaveMode = CqrsSaveMode.PerRequestTransaction`

### ZenithArch.Abstractions.Interfaces

- `IAggregateRoot`: Aggregate marker with domain-event buffer contract.
  - Example: `public partial class Order : EntityBase, IAggregateRoot { }`
- `IDomainEvent`: Domain event contract with occurrence timestamp.
  - Example: `public sealed record TripCreated(Guid TripId) : DomainEvent;`
- `IAuditable`: Audit metadata contract.
  - Example: `public DateTime CreatedAt { get; set; }`
- `ISoftDelete`: Soft-delete contract.
  - Example: `public bool IsDeleted { get; set; }`
- `ISpecification<T>`: Query specification contract.
  - Example: `public Expression<Func<Trip, bool>>? Criteria { get; }`
- `ISecurityContext`: User/tenant context abstraction.
  - Example: `public string? UserId => _http.User.Identity?.Name;`
- `IZenithArchExecutionObserver`: Runtime observer hooks for telemetry.
  - Example: `public void OnValidationFailed(string requestName, int failureCount) { ... }`

### ZenithArch.Abstractions.Base

- `EntityBase`: Aggregate base type with `Id`, domain event buffering, and clear operations.
  - Example: `public partial class Trip : EntityBase { }`
- `DomainEvent`: Base immutable domain event record.
  - Example: `public sealed record TripUpdated(Guid TripId) : DomainEvent;`

## Configuration

### ArchitectureAttribute options

| Option | Type | Default | Valid values |
| --- | --- | --- | --- |
| `Profile` | `ArchitectureProfile` | `Custom` | `Custom`, `CqrsQuickStart`, `RepositoryQuickStart`, `FullStackQuickStart` |
| `Pattern` | `ArchitecturePattern` | `Cqrs` | `Cqrs`, `Repository`, `FullStack` |
| `UseSpecification` | `bool` | `false` | `true` / `false` |
| `UseUnitOfWork` | `bool` | `false` | `true` / `false` |
| `EnableValidation` | `bool` | `false` | `true` / `false` |
| `GenerateDependencyInjection` | `bool` | `false` | `true` / `false` |
| `GenerateEndpoints` | `bool` | `false` | `true` / `false` |
| `EnableExperimentalEndpoints` | `bool` | `false` | `true` / `false` |
| `GenerateDtos` | `bool` | `false` | `true` / `false` |
| `GenerateEfConfigurations` | `bool` | `false` | `true` / `false` |
| `GenerateCachingDecorators` | `bool` | `false` | `true` / `false` |
| `GeneratePagination` | `bool` | `false` | `true` / `false` |
| `DbContextType` | `Type?` | `null` | Any `DbContext` type |
| `CqrsSaveMode` | `CqrsSaveMode` | `PerHandler` | `PerHandler`, `PerRequestTransaction` |

### Feature dependency expectations

- CQRS or FullStack generation: `MediatR`
- Validation generation: `FluentValidation`
- Persistence generation: `Microsoft.EntityFrameworkCore`
- Endpoint generation: `Microsoft.AspNetCore.App`
- Caching decorators: `Microsoft.Extensions.Caching.*`

## Versioning Policy

Zenith Arch follows SemVer (`MAJOR.MINOR.PATCH`).

- Breaking public API changes require a major version bump.
- New backward-compatible capabilities use a minor version bump.
- Backward-compatible fixes and process improvements use a patch bump.

Full change history: [CHANGELOG.md](https://github.com/tuanngp/ZenithArch/blob/main/CHANGELOG.md)

## Contributing

Issues and pull requests are welcome.

1. Open an issue with repro steps and expected behavior.
2. Fork and create a feature branch.
3. Add tests for behavior changes.
4. Submit a pull request with changelog updates.

Contribution guidelines: [CONTRIBUTING.md](https://github.com/tuanngp/ZenithArch/blob/main/CONTRIBUTING.md)

## License

MIT. See [LICENSE](https://github.com/tuanngp/ZenithArch/blob/main/LICENSE).
