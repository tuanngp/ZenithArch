# Releasing

## Local release flow

Use the repository script to bump the central version, test, pack, and optionally push packages:

```powershell
./publish.ps1 -Increment Patch
./publish.ps1 -Increment Minor
./publish.ps1 -Increment Major -Push
```

## Automated release flow

- CI validates restore, build, test, and pack on pushes and pull requests.
- Tag pushes matching `v*` trigger the release workflow.
- The release workflow pushes packages only when `NUGET_API_KEY` is configured in repository secrets.

## Release checklist

- Update `CHANGELOG.md`.
- Confirm `README.md` examples still match the current package version and support matrix.
- Run `dotnet test RynorArch.slnx`.
- Verify the artifacts in `artifacts/` before publishing.
