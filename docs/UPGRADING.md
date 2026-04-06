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
3. Validate soft-delete, auditable, and specification behavior in one representative module before wider rollout.

## Versioning policy

- Patch: diagnostics, metadata, docs, or generated output fixes that should not require consumer rewrites.
- Minor: new optional generator features or additive output.
- Major: breaking generated API shape, changed conventions, or removed feature flags.

## Consumer safety tips

- Do not auto-upgrade generator packages in production applications.
- Keep at least one sample project pinned to the current production version for comparison.
- Treat generator output changes as potentially breaking even when compile succeeds.
