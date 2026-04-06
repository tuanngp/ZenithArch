# Contributing

Thanks for contributing to RynorArch.

## Development setup

1. Install the .NET 10 SDK.
2. Restore and build the solution:

```bash
dotnet restore RynorArch.slnx
dotnet build RynorArch.slnx
```

3. Run the generator regression tests before opening a pull request:

```bash
dotnet test RynorArch.slnx
```

## Pull request expectations

- Keep changes focused and small when possible.
- Add or update tests when generator behavior changes.
- Update `README.md`, `CHANGELOG.md`, or docs in `docs/` when public behavior changes.
- Keep diagnostics actionable and avoid silent fallbacks where possible.

## Coding guidelines

- Prefer deterministic generated output.
- Avoid hidden runtime requirements in generated code.
- Treat generator diagnostics as part of the public developer experience.
