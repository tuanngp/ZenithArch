using RynorArch.Abstractions.Attributes;
using RynorArch.Abstractions.Enums;

[assembly: Architecture(
    Profile = ArchitectureProfile.FullStackQuickStart,
    Pattern = ArchitecturePattern.FullStack,
    GenerateDependencyInjection = true,
    GenerateEndpoints = true,
    EnableExperimentalEndpoints = true,
    GenerateCachingDecorators = true,
    CqrsSaveMode = CqrsSaveMode.PerRequestTransaction
)]
