# Upgrading

## Upgrade workflow

1. Update package versions in a branch.
2. Run `dotnet test RynorArch.slnx`.
3. Compare generated output for a representative sample project.
4. Read `CHANGELOG.md` for behavior changes and diagnostics updates.
5. Roll out to additional modules only after the first upgraded module is stable.

## Hybrid refactor migration notes

The current generator reduces per-entity source size by moving shared CRUD and EF Core interaction into a generated generic infrastructure layer.

This means:

- generated repositories are now thin wrappers over `CrudRepository<TEntity>`
- CQRS handlers still exist per entity, but their persistence logic delegates to shared helpers
- the generated infrastructure is emitted once per compilation as a shared support file

## Breaking change impact

You may need code changes if you previously:

- depended on the exact body or members of generated repository implementations
- subclassed or copied generated repository implementations
- assumed every entity would get a full standalone CRUD implementation in generated output

## Migration approach

1. Replace any dependency on generated repository internals with the repository interface or the public generated handler types.
2. Re-run the build and inspect the new generated infrastructure file plus the thinner repository output.
3. Validate soft-delete, auditable, specification, and CQRS save-mode behavior in one representative module before wider rollout.

## Deep optimization notes

The latest optimization pass is intended to be mostly internal and should preserve the public generated type shape from the hybrid refactor.

Key changes:

- shared CRUD runtime now caches per-entity traits for repeated query paths
- specification application is split internally into list/count branches with the same public API as before
- `IUnitOfWork` is emitted once per compilation instead of once per entity
- CQRS list filtering and generated specification filtering now use shared generation rules to reduce drift

Most consumers should not need migration changes beyond rebuilding and validating representative generated output.

## Versioning policy

- Patch: diagnostics, metadata, docs, or generated output fixes that should not require consumer rewrites.
- Minor: new optional generator features or additive output.
- Major: breaking generated API shape, changed conventions, or removed feature flags.

## Consumer safety tips

- Do not auto-upgrade generator packages in production applications.
- Keep at least one sample project pinned to the current production version for comparison.
- Treat generator output changes as potentially breaking even when compile succeeds.

## Developer experience checks during upgrade

After upgrading, validate observability and diagnostics in addition to compile success:

1. Confirm `RynorArch.GenerationReport.g.cs` is emitted and lists expected entities/artifacts.
2. Check generated headers for `rynor-artifact` metadata to ensure traceability is intact.
3. Review `RYNOR007`-`RYNOR012` diagnostics and resolve all errors before rollout.
4. If CQRS is enabled, validate `DbContextType` (if set) resolves to a real `DbContext` (`RYNOR008`).
5. If endpoint generation is enabled, confirm explicit experimental opt-in is present (`RYNOR012`).
