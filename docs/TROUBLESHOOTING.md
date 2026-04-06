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

## Generated files in source control

- Keep generated files out of source control by default.
- Commit generated files only if your team has a deliberate review or diffing workflow that depends on them.

## Build issues after upgrades

- Clear `bin/` and `obj/` folders.
- Restore packages again.
- Compare generated output before and after the upgrade.
- Re-run `dotnet test RynorArch.slnx`.
