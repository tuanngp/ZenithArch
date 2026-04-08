# Integration Guide

This document shows practical startup wiring for generated artifacts.

## CQRS / FullStack setup

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using RynorArch.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddRynorArchDependencies();

var app = builder.Build();
app.Run();
```

Notes:
- `AddRynorArchDependencies()` can register MediatR automatically by default.
- If using custom MediatR setup, call `AddRynorArchDependencies(registerMediatR: false)`.

## Repository setup

```csharp
using Microsoft.EntityFrameworkCore;
using RynorArch.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddRynorArchDependencies<AppDbContext>();
```

Notes:
- Prefer `AddRynorArchDependencies<TDbContext>()` when `UseUnitOfWork = true` so generated `IUnitOfWork` is auto-wired.
- The non-generic overload remains valid when you do not use UnitOfWork.

## Endpoint generation

If enabled:

```csharp
using RynorArch.Endpoints;

app.MapRynorArchEndpoints();
```

Remember endpoint generation requires both:
- `GenerateEndpoints = true`
- `EnableExperimentalEndpoints = true`

Before production rollout, apply the checklist in `docs/ENDPOINT_HARDENING.md`.

## Caching decorators

When `GenerateCachingDecorators = true`:
- register a distributed cache provider (for example Redis or memory-backed distributed cache)
- call `AddRynorArchDependencies()` so generated cache behaviors and invalidators are wired
- follow `docs/CACHING_OPERATIONS.md` for TTL, key design, and rollout guardrails

## Save mode

If `CqrsSaveMode = CqrsSaveMode.PerRequestTransaction`:
- keep generated DI wiring enabled, or
- manually register `IPipelineBehavior<,>` to `RynorArchSaveChangesBehavior<,>`

## AI-agent integration pattern

When an agent is implementing changes, use this minimal gate:

1. Apply configuration and code changes.
2. Run `dotnet build`.
3. Run `rynor doctor`.
4. Only continue if summary is `READY` or `READY WITH WARNINGS`.

For full task contracts and output expectations, see `docs/AI_AGENT_PLAYBOOK.md`.
