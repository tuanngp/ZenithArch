namespace ZenithArch.Abstractions.Enums;

/// <summary>
/// Defines the architecture pattern the source generator will enforce.
/// </summary>
public enum ArchitecturePattern
{
    /// <summary>
    /// Generates Commands, Queries, and Handlers using MediatR.
    /// Uses DbContext directly. Does NOT generate Repository.
    /// </summary>
    Cqrs = 0,

    /// <summary>
    /// Generates Repository, UnitOfWork, and Specification patterns.
    /// Does NOT generate CQRS.
    /// </summary>
    Repository = 1,

    /// <summary>
    /// Generates all components: CQRS + Repository + Specification.
    /// Must be explicitly enabled.
    /// </summary>
    FullStack = 2,
}
