using RynorArch.Abstractions.Attributes;
using RynorArch.Abstractions.Enums;

[assembly: Architecture(
    Pattern = ArchitecturePattern.FullStack,
    GenerateDependencyInjection = true,
    GenerateEndpoints = true,
    GenerateDtos = true,
    GenerateEfConfigurations = true,
    GenerateCachingDecorators = true,
    GeneratePagination = true,
    EnableValidation = true,
    UseSpecification = true,
    UseUnitOfWork = true
)]
