using RynorArch.Abstractions.Attributes;
using RynorArch.Abstractions.Enums;

[assembly: Architecture(
    Pattern = ArchitecturePattern.Cqrs,
    UseSpecification = true,
    UseUnitOfWork = false,
    EnableValidation = true
)]
