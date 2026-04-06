# Upgrading

## Upgrade workflow

1. Update package versions in a branch.
2. Run `dotnet test RynorArch.slnx`.
3. Compare generated output for a representative sample project.
4. Read `CHANGELOG.md` for behavior changes and diagnostics updates.
5. Roll out to additional modules only after the first upgraded module is stable.

## Versioning policy

- Patch: diagnostics, metadata, docs, or generated output fixes that should not require consumer rewrites.
- Minor: new optional generator features or additive output.
- Major: breaking generated API shape, changed conventions, or removed feature flags.

## Consumer safety tips

- Do not auto-upgrade generator packages in production applications.
- Keep at least one sample project pinned to the current production version for comparison.
- Treat generator output changes as potentially breaking even when compile succeeds.
