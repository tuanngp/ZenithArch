# Troubleshooting

[Tiếng Việt](TROUBLESHOOTING.md) | [English](TROUBLESHOOTING.en.md)


## Common diagnostics

### `ZENITH001` No entities found

No class with `[Entity]` was discovered in the current compilation.

Check:
- the project references `ZenithArch.Abstractions`
- the entity class is part of the current project
- the entity is marked with `[Entity]`

### `ZENITH002` AggregateRoot requires Entity

`[AggregateRoot]` only works on classes that also have `[Entity]`.

### `ZENITH003` Architecture pattern conflict

The selected feature flags do not match the chosen architecture pattern.

Examples:
- `UseUnitOfWork = true` with `Cqrs`
- `EnableValidation = true` with `Repository`

### `ZENITH004` Unsupported QueryFilter type

`[QueryFilter]` is intended for string, numeric, boolean, `DateTime`, `Guid`, enums, and nullable variants of those types.

If you need complex filtering, remove `[QueryFilter]` and implement the logic manually in a partial handler or specification.

### `ZENITH005` Entity must be partial

Generated extensions and partial hooks require `partial` entities.

### `ZENITH006` Missing architecture configuration

Add an explicit assembly-level configuration such as:

```csharp
using ZenithArch.Abstractions.Attributes;
using ZenithArch.Abstractions.Enums;

[assembly: Architecture(Pattern = ArchitecturePattern.Cqrs)]
```

Zenith Arch does not generate sources without explicit architecture configuration.

### `ZENITH007` Missing required dependency

One or more enabled features require packages/framework references that are not available to the compilation.
The diagnostic now includes an exact `PackageReference` or `FrameworkReference` hint.

Common examples:
- CQRS without `MediatR`
- Validation enabled without `FluentValidation`
- Persistence features without `Microsoft.EntityFrameworkCore`
- Endpoints enabled without `Microsoft.AspNetCore.App`
- Caching decorators enabled without `Microsoft.Extensions.Caching.*`

### `ZENITH008` Configured DbContext type is invalid

`DbContextType` was configured but the type cannot be resolved or does not derive from `Microsoft.EntityFrameworkCore.DbContext`.

Fix options:
- set `DbContextType = typeof(YourDbContext)` to a valid type in the compilation
- remove `DbContextType` to fall back to `Microsoft.EntityFrameworkCore.DbContext`

### `ZENITH009` Generated endpoint behavior notice

Endpoint generation is active and compilation succeeded, but the generated endpoints are intentionally minimal.
Harden them for enterprise APIs (authorization, richer error contracts, and resource-not-found semantics).
Follow the hardening checklist in `docs/ENDPOINT_HARDENING.md`.

### `ZENITH010` Generated cache behavior notice

Generated cache pipeline behaviors include per-entity invalidation contracts.
Ensure invalidators are registered in DI (generated DI helper does this when enabled).
Operational rollout guidance is in `docs/CACHING_OPERATIONS.md`.

### Validation is enabled but invalid commands still pass

If `EnableValidation = true`, generated DI should register `ZenithArchValidationBehavior<,>`.

Check:
- `AddZenithArchDependencies()` is called at startup, or equivalent manual registrations are present.
- MediatR is configured for the same assembly containing generated handlers.
- Generated validator files (`*.Validation.g.cs`) exist under `obj/`.

If `GenerateDependencyInjection = false`, manually register `IPipelineBehavior<,>` to `ZenithArchValidationBehavior<,>`.

### Generated endpoints return unexpected write status codes

Current generated behavior:
- `POST` returns `201 Created` with `{ id = <guid> }` payload.
- `PUT`/`DELETE` return `404` when the resource is missing and `204` when successful.

If you still observe unconditional `204`, rebuild and verify the generated `ZenithArchEndpointExtensions.g.cs` in `obj/`.

### `ZENITH011` Feature flag ignored by selected pattern

A feature flag was enabled, but the selected architecture pattern does not support generating that artifact.
Align pattern and flags in `[assembly: Architecture(...)]` to silence this warning.

### `ZENITH012` Endpoint generation requires experimental opt-in

`GenerateEndpoints` is enabled but `EnableExperimentalEndpoints` is not.

Fix by opting in explicitly:

```csharp
[assembly: Architecture(
	Pattern = ArchitecturePattern.Cqrs,
	GenerateEndpoints = true,
	EnableExperimentalEndpoints = true
)]
```

### `ZENITH013` CQRS save mode needs generated DI wiring

`CqrsSaveMode.PerRequestTransaction` is enabled while `GenerateDependencyInjection` is false.

Fix options:
- set `GenerateDependencyInjection = true`, or
- manually register `IPipelineBehavior<,>` to `ZenithArchSaveChangesBehavior<,>`

### `ZENITH014` Consider starter profile migration

Your module still uses a legacy explicit-flag style configuration.

Fix options:
- set `Profile = ArchitectureProfile.CqrsQuickStart` / `RepositoryQuickStart` / `FullStackQuickStart`
- keep only explicit flags that intentionally override profile defaults

See `docs/UPGRADING_PROFILES.md` for migration mapping examples.

### `ZENITH015` Validation needs generated DI wiring

`EnableValidation` is enabled while `GenerateDependencyInjection` is false.

Fix options:
- set `GenerateDependencyInjection = true`, or
- manually register `IPipelineBehavior<,>` to `ZenithArchValidationBehavior<,>`

### `ZENITH016` Endpoint hardening checklist recommended

`GenerateEndpoints` is enabled and endpoint generation succeeded.
This informational diagnostic reminds you to apply production hardening before rollout.

Check `docs/ENDPOINT_HARDENING.md` and verify at minimum:
- authorization boundaries (`RequireAuthorization` and policy split)
- consistent problem details / exception mapping
- observability (structured logs, traces, metrics)
- API lifecycle protections (versioning, idempotency where needed)

## Debugging generated output

- Inspect `ZenithArch.GenerationReport.g.cs` to see what artifacts were emitted.
- Check file headers for `rynor-artifact`, `rynor-entity`, and `rynor-assumptions` metadata.
- Use diagnostics as the primary signal before debugging emitted code bodies.

## Generated files in source control

- Keep generated files out of source control by default.
- Commit generated files only if your team has a deliberate review or diffing workflow that depends on them.

## Build issues after upgrades

- Clear `bin/` and `obj/` folders.
- Restore packages again.
- Compare generated output before and after the upgrade.
- Re-run `dotnet test ZenithArch.slnx`.

## CLI doctor troubleshooting

Use `rynor doctor` as a readiness gate for automated workflows.

- `DR002` Project file fail: run command in a project folder containing a `.csproj`.
- `DR004` Architecture config fail: add `AssemblyConfig.cs` with `[assembly: Architecture(...)]`.
- `DR006` Endpoint opt-in fail: set `EnableExperimentalEndpoints = true` when endpoints are enabled.
- `DR009`-`DR013` dependency fail: add the package/framework hinted in output.
- `DR014` entity fail: mark all `[Entity]` classes as `partial`.
- `DR015` report warning: build once so generated report is emitted under `obj/`.

Reference contracts and verification flow are in `docs/AI_AGENT_PLAYBOOK.md`.
