# Troubleshooting

## Common diagnostics

### `RYNOR001` No entities found

No class with `[Entity]` was discovered in the current compilation.

Check:
- the project references `RynorArch.Abstractions`
- the entity class is part of the current project
- the entity is marked with `[Entity]`

### `RYNOR002` AggregateRoot requires Entity

`[AggregateRoot]` only works on classes that also have `[Entity]`.

### `RYNOR003` Architecture pattern conflict

The selected feature flags do not match the chosen architecture pattern.

Examples:
- `UseUnitOfWork = true` with `Cqrs`
- `EnableValidation = true` with `Repository`

### `RYNOR004` Unsupported QueryFilter type

`[QueryFilter]` is intended for string, numeric, boolean, `DateTime`, `Guid`, enums, and nullable variants of those types.

If you need complex filtering, remove `[QueryFilter]` and implement the logic manually in a partial handler or specification.

### `RYNOR005` Entity must be partial

Generated extensions and partial hooks require `partial` entities.

### `RYNOR006` Missing architecture configuration

Add an explicit assembly-level configuration such as:

```csharp
using RynorArch.Abstractions.Attributes;
using RynorArch.Abstractions.Enums;

[assembly: Architecture(Pattern = ArchitecturePattern.Cqrs)]
```

### `RYNOR007` Missing required dependency

One or more enabled features require packages/framework references that are not available to the compilation.

Common examples:
- CQRS without `MediatR`
- Validation enabled without `FluentValidation`
- Persistence features without `Microsoft.EntityFrameworkCore`
- Endpoints enabled without `Microsoft.AspNetCore.App`
- Caching decorators enabled without `Microsoft.Extensions.Caching.*`

### `RYNOR008` AppDbContext convention not satisfied

CQRS generation expects an `AppDbContext : DbContext` type in the compilation.

Fix options:
- Rename your existing DbContext to `AppDbContext`, or
- Introduce an `AppDbContext` wrapper type for generated handlers, or
- Switch to a non-CQRS pattern and wire handlers manually.

### `RYNOR009` Generated endpoint behavior notice

Endpoint generation is active and compilation succeeded, but the generated endpoints are intentionally minimal.
Harden them for enterprise APIs (authorization, richer error contracts, and resource-not-found semantics).

### `RYNOR010` Generated cache behavior notice

Generated cache pipeline behaviors do not emit automatic invalidation for write flows.
Implement invalidation in command handlers/pipeline behaviors before using generated caches in production.

### `RYNOR011` Feature flag ignored by selected pattern

A feature flag was enabled, but the selected architecture pattern does not support generating that artifact.
Align pattern and flags in `[assembly: Architecture(...)]` to silence this warning.

## Debugging generated output

- Inspect `RynorArch.GenerationReport.g.cs` to see what artifacts were emitted.
- Check file headers for `rynor-artifact`, `rynor-entity`, and `rynor-assumptions` metadata.
- Use diagnostics as the primary signal before debugging emitted code bodies.

## Generated files in source control

- Keep generated files out of source control by default.
- Commit generated files only if your team has a deliberate review or diffing workflow that depends on them.

## Build issues after upgrades

- Clear `bin/` and `obj/` folders.
- Restore packages again.
- Compare generated output before and after the upgrade.
- Re-run `dotnet test RynorArch.slnx`.
