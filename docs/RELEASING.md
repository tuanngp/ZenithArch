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
- Run `dotnet restore RynorArch.slnx`.
- Run `dotnet build RynorArch.slnx -c Release`.
- Run `dotnet test RynorArch.slnx -c Release`.
- Run `dotnet test tests/RynorArch.Integration.Tests/RynorArch.Integration.Tests.csproj -c Release`.
- Run `dotnet run --project src/RynorArch.Cli/RynorArch.Cli.csproj -- doctor samples/RynorArch.Sample`.
- Confirm runtime scenarios in `docs/RUNTIME_TESTING.md` are covered by green tests.
- Verify the artifacts in `artifacts/` before publishing.
