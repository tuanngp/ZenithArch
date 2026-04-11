# Attribute Reference

[Tiếng Việt](ATTRIBUTE_REFERENCE.md) | [English](ATTRIBUTE_REFERENCE.en.md)


## Architecture

Assembly-level configuration for generator behavior.

```csharp
[assembly: Architecture(Profile = ArchitectureProfile.CqrsQuickStart)]
```

Key fields:
- `Profile`: low-touch starter defaults.
- `Pattern`: `Cqrs`, `Repository`, or `FullStack`.
- `GenerateDependencyInjection`: emit DI extension and runtime registrations.
- `GenerateEndpoints` + `EnableExperimentalEndpoints`: endpoint opt-in pair.
- `GenerateCachingDecorators`: read caching + invalidation artifacts.
- `DbContextType`: optional CQRS handler constructor type override.
- `CqrsSaveMode`: write persistence strategy.

## Entity

Marks a type for generation.

Requirements:
- class must be `partial`
- type should derive from `EntityBase` for common conventions

## AggregateRoot

Enables domain event artifact generation for the entity.

Requirement:
- type must also be marked with `[Entity]`

## QueryFilter

Marks a property as filterable in generated list query/specification logic.

Supported property types:
- string
- numeric primitives
- bool
- DateTime
- Guid
- enums
- nullable variants of the above

## Validation attributes

These are RynorArch attributes (not `System.ComponentModel.DataAnnotations`):
- `[Required]`
- `[MinLength(n)]`
- `[MaxLength(n)]`
- `[Email]`

They drive generated FluentValidation rules when validation generation is enabled.
