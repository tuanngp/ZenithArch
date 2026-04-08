# Getting Started

This guide is optimized for the shortest path to a working setup.

## 1. Install packages

```xml
<PackageReference Include="RynorArch.Abstractions" Version="1.0.6" />
<PackageReference Include="RynorArch.Generator" Version="1.0.6" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

## 2. Create architecture config

### Option A: CLI (fastest)

```bash
rynor init
```

### Option B: Manual

Create `AssemblyConfig.cs`:

```csharp
using RynorArch.Abstractions.Attributes;
using RynorArch.Abstractions.Enums;

[assembly: Architecture(
    Profile = ArchitectureProfile.CqrsQuickStart,
    Pattern = ArchitecturePattern.Cqrs,
    GenerateDependencyInjection = true
)]
```

## 3. Add an entity

```csharp
using RynorArch.Abstractions.Attributes;
using RynorArch.Abstractions.Base;

namespace MyApp.Domain;

[Entity]
public partial class Trip : EntityBase
{
    [QueryFilter]
    public string Destination { get; set; } = string.Empty;
}
```

## 4. Build

```bash
dotnet build
```

Inspect generated output in `obj/` and `RynorArch.GenerationReport.g.cs`.

## 5. Wire runtime

In your app startup:

```csharp
builder.Services.AddRynorArchDependencies();
```

If `UseUnitOfWork = true` (Repository/FullStack), prefer:

```csharp
builder.Services.AddRynorArchDependencies<AppDbContext>();
```

The generated DI extension registers handlers/repositories (by pattern), validators, and cache behaviors (if enabled).

## Common first-run issues

- `RYNOR005`: entity marked `[Entity]` must be `partial`.
- `RYNOR006`: missing `AssemblyConfig.cs` or missing `[assembly: Architecture(...)]`.
- `RYNOR007`: missing package dependency for an enabled feature flag.

## Getting started with AI agents

Use this sequence when an AI agent is driving setup:

1. Run `rynor init`.
2. Run `rynor scaffold Trip MyApp.Domain`.
3. Run `dotnet build`.
4. Run `rynor doctor` and resolve all FAIL checks.

See `docs/AI_AGENT_PLAYBOOK.md` for task contracts and verification rules.
