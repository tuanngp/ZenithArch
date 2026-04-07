# Feature Matrix

| Feature | Cqrs | Repository | FullStack | Notes |
| --- | --- | --- | --- | --- |
| UseSpecification | Yes | Yes | Yes | Generates specification artifacts from `[QueryFilter]` |
| UseUnitOfWork | No | Yes | Yes | Ignored in pure CQRS |
| EnableValidation | Yes | No | Yes | Generates FluentValidation validators for commands |
| GenerateDependencyInjection | Yes | Yes | Yes | Emits `AddRynorArchDependencies()` |
| GenerateEndpoints | Yes | No | Yes | Requires `EnableExperimentalEndpoints = true` |
| GenerateDtos | Yes | Yes | Yes | DTO records and mapping extensions |
| GenerateEfConfigurations | Yes | Yes | Yes | Entity type configuration partials |
| GenerateCachingDecorators | Yes | No | Yes | Adds query cache behaviors + invalidators |
| GeneratePagination | Yes | Yes | Yes | Pagination extension artifacts |
| CqrsSaveMode | Yes | No | Yes | `PerHandler` or `PerRequestTransaction` |
| DbContextType | Yes | N/A | Yes | Optional override for generated CQRS handler constructor type |

## Starter profiles

| Profile | Intended setup | Defaults summary |
| --- | --- | --- |
| CqrsQuickStart | API/service modules using command-query handlers | CQRS + validation + specification + generated DI |
| RepositoryQuickStart | Layered modules preferring repository boundary | Repository + specification + unit of work + generated DI |
| FullStackQuickStart | End-to-end modules wanting both CQRS and repository artifacts | FullStack + common productivity flags + generated DI |

Profile defaults are only a starting point. Explicit flags in `Architecture(...)` always override profile values.
