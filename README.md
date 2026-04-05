# RynorArch

**RynorArch** is a compile-time .NET architecture automation framework using Roslyn Incremental Source Generators. It is designed to eliminate boilerplate code for .NET 9 applications while enforcing strict clean architecture patterns (CQRS, Repository, or FullStack) with zero runtime overhead and Native AOT compatibility.

## Features

- **Roslyn Incremental Source Generator**: Extremely fast, highly cached, only rebuilds what's necessary.
- **Zero Runtime Reflection**: Generated code operates directly on your types.
- **Deterministic Output**: Consistent, predictable output files (`.g.cs`).
- **Clean Architecture Enforced**: Strict separation of concerns depending on the chosen pattern. Handlers are decoupled from repositories unless FullStack is explicitly used.
- **Extensible via Partial Classes**: All generated handlers, repositories, and validators are `partial` and provide lifecycle hooks (`OnBeforeHandle`, `OnValidate`, etc.) for you to inject custom logic.
- **DDD Integration**: First-class support for `[AggregateRoot]` and Domain Events.

## Installation

Add references to the core packages in your `.csproj`:

```xml
<!-- The public API package (contains attributes, interfaces) -->
<PackageReference Include="RynorArch.Abstractions" Version="1.0.0" />

<!-- The source generator package (development dependency only) -->
<PackageReference Include="RynorArch.Generator" Version="1.0.0" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

> **Note**: CQRS mode relies on MediatR and FluentValidation (if enabled). Ensure you include these dependencies in your project as well.
```xml
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="FluentValidation" Version="11.11.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
```

## Configuration

RynorArch uses an assembly-level configuration attribute to determine its behavior. Place this in any `.cs` file in your main project (e.g., `AssemblyConfig.cs`):

```csharp
using RynorArch.Abstractions.Attributes;
using RynorArch.Abstractions.Enums;

[assembly: Architecture(
    Pattern = ArchitecturePattern.Cqrs,
    UseSpecification = true,
    UseUnitOfWork = false,
    EnableValidation = true
)]
```

### Supported Patterns

- `ArchitecturePattern.Cqrs` - Generates MediatR Commands, Queries, and Handlers. Handlers dependency-inject the DbContext directly.
- `ArchitecturePattern.Repository` - Generates `IRepository<T>` interfaces, concrete Repository implementations, and optionally a `UnitOfWork` interface.
- `ArchitecturePattern.FullStack` - Generates both CQRS and Repository artifacts.

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
2. **Handlers**: `CreateTripHandler`, `UpdateTripHandler`, etc.
3. **Specifications**: `TripSpecification` mapped to `[QueryFilter]` properties.
4. **Validators**: `CreateTripValidator`, `UpdateTripValidator` stubs with basic rules implemented.
5. **Domain Events**: `TripCreatedEvent`, `TripUpdatedEvent`, `TripDeletedEvent` (because of `[AggregateRoot]`).

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

## Built With

- **.NET 9 SDK**
- **Roslyn Incremental Source Generators** API (`IIncrementalGenerator`)
- **MediatR**
- **FluentValidation**
- **Entity Framework Core**

---
## Packing as NuGet

Since building Source Generators generally targets `netstandard2.0` and comes packaged differently, `RynorArch` uses `.csproj` properties optimally out of the box to export analyzing DLLs inside `.nupkg`.

To pack this solution into multiple redistributable NuGet packages, run the following:

```bash
dotnet pack --configuration Release
```

Two artifacts will be generated:
1. `src/RynorArch.Abstractions/bin/Release/RynorArch.Abstractions.1.0.0.nupkg`
2. `src/RynorArch.Generator/bin/Release/RynorArch.Generator.1.0.0.nupkg`

You can manually distribute these or publish to Nuget via:
```bash
dotnet nuget push src/RynorArch.Generator/bin/Release/RynorArch.Generator.1.0.0.nupkg --api-key <YOUR_API_KEY> --source https://api.nuget.org/v3/index.json
```
