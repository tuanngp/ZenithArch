# Upgrading To Profile-First Configuration

Legacy configurations with many explicit flags still work, but profile-first setup is easier to maintain.

## Why migrate

- Fewer repetitive flags across services.
- Lower chance of config drift between modules.
- Simpler onboarding and code review.

## Mapping guide

- CQRS-heavy modules: `ArchitectureProfile.CqrsQuickStart`
- Repository-heavy modules: `ArchitectureProfile.RepositoryQuickStart`
- End-to-end modules: `ArchitectureProfile.FullStackQuickStart`

## Example migration

Before:

```csharp
[assembly: Architecture(
    Pattern = ArchitecturePattern.FullStack,
    UseSpecification = true,
    UseUnitOfWork = true,
    EnableValidation = true,
    GenerateDependencyInjection = true,
    GenerateDtos = true,
    GenerateEfConfigurations = true,
    GeneratePagination = true,
    CqrsSaveMode = CqrsSaveMode.PerRequestTransaction
)]
```

After:

```csharp
[assembly: Architecture(
    Profile = ArchitectureProfile.FullStackQuickStart,
    Pattern = ArchitecturePattern.FullStack,
    GenerateDependencyInjection = true,
    CqrsSaveMode = CqrsSaveMode.PerRequestTransaction
)]
```

Use explicit flags only for intentional overrides from profile defaults.

## Migration checklist

1. Set the closest `Profile` for each module.
2. Remove flags that match the profile default behavior.
3. Keep only flags that intentionally diverge.
4. Rebuild and compare `RynorArch.GenerationReport.g.cs`.
5. Run integration tests for one pilot module before broad rollout.
