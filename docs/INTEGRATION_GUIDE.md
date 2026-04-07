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

builder.Services.AddRynorArchDependencies();
```

## Endpoint generation

If enabled:

```csharp
using RynorArch.Endpoints;

app.MapRynorArchEndpoints();
```

Remember endpoint generation requires both:
- `GenerateEndpoints = true`
- `EnableExperimentalEndpoints = true`

## Caching decorators

When `GenerateCachingDecorators = true`:
- register a distributed cache provider (for example Redis or memory-backed distributed cache)
- call `AddRynorArchDependencies()` so generated cache behaviors and invalidators are wired

## Save mode

If `CqrsSaveMode = CqrsSaveMode.PerRequestTransaction`:
- keep generated DI wiring enabled, or
- manually register `IPipelineBehavior<,>` to `RynorArchSaveChangesBehavior<,>`
