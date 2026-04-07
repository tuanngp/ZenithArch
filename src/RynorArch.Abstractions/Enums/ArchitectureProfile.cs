namespace RynorArch.Abstractions.Enums;

/// <summary>
/// Provides low-touch starter profiles for common architecture setups.
/// Individual flags on ArchitectureAttribute can still override profile defaults.
/// </summary>
public enum ArchitectureProfile
{
    /// <summary>
    /// No profile defaults are applied. Use explicit per-flag configuration.
    /// </summary>
    Custom = 0,

    /// <summary>
    /// CQRS-focused profile for service modules.
    /// Enables specification, validation, and dependency injection generation.
    /// </summary>
    CqrsQuickStart = 1,

    /// <summary>
    /// Repository-focused profile for layered applications.
    /// Enables specification, unit of work, and dependency injection generation.
    /// </summary>
    RepositoryQuickStart = 2,

    /// <summary>
    /// Full-stack profile for end-to-end modules.
    /// Enables CQRS + repository generation with common productivity features.
    /// </summary>
    FullStackQuickStart = 3,
}
