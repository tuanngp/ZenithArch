# Compatibility

## Officially validated development environment

- .NET SDK: `10.0.x`
- RynorArch generator target: `netstandard2.0`
- RynorArch CLI target runtimes: `net8.0`, `net9.0`, `net10.0`

## Consumer project expectations

- Add `RynorArch.Abstractions` as a normal package reference.
- Add `RynorArch.Generator` as an analyzer-only package reference.
- Declare `[assembly: Architecture(...)]` explicitly to avoid relying on defaults.
- Expect a generated shared infrastructure layer for CRUD/EF interaction when using `Cqrs`, `Repository`, or `FullStack`.

## Feature dependencies

| Feature | Required dependency |
| --- | --- |
| CQRS / FullStack handlers | `MediatR` |
| Validation generation | `FluentValidation` |
| Repository / CQRS persistence | `Microsoft.EntityFrameworkCore` |
| Endpoint generation | ASP.NET Core shared framework |
| Caching decorators | `Microsoft.Extensions.Caching.*` |

## Recommended adoption pattern

- Start with a single module or bounded context.
- Pin the package version in production apps.
- Review generated output during upgrades.
- If your code referenced generated repository internals directly, plan a migration because repository implementations are now thin wrappers over a generic base.
