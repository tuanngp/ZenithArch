# AI Agent Playbook

[Tiếng Việt](AI_AGENT_PLAYBOOK.md) | [English](AI_AGENT_PLAYBOOK.en.md)


This playbook defines deterministic workflows for AI agents working with Zenith Arch projects.

## Workflow Contract

Use this contract for every agent task:

1. Input requirements: required files, required config, required dependencies.
2. Expected output: generated artifacts or code paths that must exist.
3. Success criteria: build/test/diagnostic checks that must pass.
4. Failure handling: exact next action when checks fail.

## Task 1: Initialize Architecture Config

### Input requirements

- A project root containing a `.csproj`.
- `ZenithArch.Abstractions` and `ZenithArch.Generator` available via package or project references.

### Execution path

1. Run `zenith init`.
2. Choose an architecture profile.
3. Confirm `AssemblyConfig.cs` exists.

### Expected output

- `AssemblyConfig.cs`
- `README_NEXT_STEPS.md`

### Success criteria

- `zenith doctor` shows no FAIL for `DR002`, `DR004`, `DR007`, `DR008`.

## Task 2: Scaffold First Entity

### Input requirements

- `AssemblyConfig.cs` already configured.

### Execution path

1. Run `zenith scaffold <EntityName> <Namespace>`.
2. Build once with `dotnet build`.

### Expected output

- `Domain/<EntityName>.cs`
- Optional CQRS extension partials under `Cqrs/<EntityName>/`
- `ZenithArch.GenerationReport.g.cs` under `obj/`

### Success criteria

- `zenith doctor` has PASS for `DR014` and `DR015`.
- Build completes without generator errors.

## Task 3: Integrate Runtime Wiring

### Input requirements

- App startup has DI setup.

### Execution path

1. Call `builder.Services.AddZenithArchDependencies();`.
2. If UnitOfWork is enabled, use `builder.Services.AddZenithArchDependencies<AppDbContext>();`.
3. If endpoint generation is enabled, call `app.MapZenithArchEndpoints();`.

### Success criteria

- Dependency checks `DR009` to `DR013` show no FAIL.
- App startup succeeds.

## Task 4: Readiness Gate

### Execution path

1. Run `zenith doctor` from project root.
2. Review summary line.

### Decision rule

- `NOT READY`: at least one FAIL, must fix before proceeding.
- `READY WITH WARNINGS`: can continue in dev, should resolve warnings before production rollout.
- `READY`: no blocking checks.

## Common Failure Map

- Missing architecture config: fix via `zenith init`.
- Endpoints without opt-in: set `EnableExperimentalEndpoints = true` or disable endpoint generation.
- Non-partial entity: mark `[Entity]` classes as `partial`.
- Missing generated report: run `dotnet build` once.

## Verification Commands

```bash
dotnet build
zenith doctor
dotnet test ZenithArch.slnx -v minimal
```

## Related Guides

- `docs/GETTING_STARTED.md`
- `docs/INTEGRATION_GUIDE.md`
- `docs/TROUBLESHOOTING.md`
- `docs/ENDPOINT_HARDENING.md`
- `docs/CACHING_OPERATIONS.md`
