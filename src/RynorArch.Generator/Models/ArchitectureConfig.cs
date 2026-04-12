using System;

namespace RynorArch.Generator.Models;

/// <summary>
/// Architecture configuration extracted from the assembly-level [Architecture] attribute.
/// Determines which emitters are activated during code generation.
/// </summary>
public sealed class ArchitectureConfig : IEquatable<ArchitectureConfig>
{
    /// <summary>
    /// Default fully qualified DbContext type used when no explicit context type is configured.
    /// </summary>
    public const string DefaultCqrsDbContextTypeName = "global::Microsoft.EntityFrameworkCore.DbContext";

    /// <summary>
    /// Gets the selected architecture profile numeric value.
    /// 0 = Custom, 1 = CqrsQuickStart, 2 = RepositoryQuickStart, 3 = FullStackQuickStart.
    /// </summary>
    public int Profile { get; }

    /// <summary>
    /// Gets the selected architecture pattern numeric value.
    /// 0 = Cqrs, 1 = Repository, 2 = FullStack.
    /// </summary>
    public int Pattern { get; }

    /// <summary>
    /// Gets a value indicating whether generated specifications are enabled.
    /// </summary>
    public bool UseSpecification { get; }

    /// <summary>
    /// Gets a value indicating whether generated unit of work infrastructure is enabled.
    /// </summary>
    public bool UseUnitOfWork { get; }

    /// <summary>
    /// Gets a value indicating whether generated validation components are enabled.
    /// </summary>
    public bool EnableValidation { get; }

    /// <summary>
    /// Gets a value indicating whether generated dependency injection registration is enabled.
    /// </summary>
    public bool GenerateDependencyInjection { get; }

    /// <summary>
    /// Gets a value indicating whether generated minimal API endpoints are enabled.
    /// </summary>
    public bool GenerateEndpoints { get; }

    /// <summary>
    /// Gets a value indicating whether experimental endpoint generation is explicitly enabled.
    /// </summary>
    public bool EnableExperimentalEndpoints { get; }

    /// <summary>
    /// Gets a value indicating whether generated DTO types are enabled.
    /// </summary>
    public bool GenerateDtos { get; }

    /// <summary>
    /// Gets a value indicating whether generated EF Core configuration classes are enabled.
    /// </summary>
    public bool GenerateEfConfigurations { get; }

    /// <summary>
    /// Gets a value indicating whether generated query caching decorators are enabled.
    /// </summary>
    public bool GenerateCachingDecorators { get; }

    /// <summary>
    /// Gets a value indicating whether generated pagination helpers are enabled.
    /// </summary>
    public bool GeneratePagination { get; }

    /// <summary>
    /// Gets the fully qualified DbContext type name used by generated CQRS handlers.
    /// </summary>
    public string CqrsDbContextTypeName { get; }

    /// <summary>
    /// Gets the selected CQRS save mode numeric value.
    /// </summary>
    public int CqrsSaveMode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchitectureConfig"/> class.
    /// </summary>
    /// <param name="pattern">Numeric architecture pattern value.</param>
    /// <param name="useSpecification">Enables specification generation when true.</param>
    /// <param name="useUnitOfWork">Enables unit of work generation when true.</param>
    /// <param name="enableValidation">Enables generated validation support when true.</param>
    /// <param name="generateDependencyInjection">Enables generated DI registration when true.</param>
    /// <param name="generateEndpoints">Enables generated endpoint mapping when true.</param>
    /// <param name="enableExperimentalEndpoints">Enables experimental endpoint generation when true.</param>
    /// <param name="generateDtos">Enables generated DTO projection support when true.</param>
    /// <param name="generateEfConfigurations">Enables generated EF Core configurations when true.</param>
    /// <param name="generateCachingDecorators">Enables generated caching decorators when true.</param>
    /// <param name="generatePagination">Enables generated pagination helpers when true.</param>
    /// <param name="cqrsDbContextTypeName">Optional fully qualified DbContext type name.</param>
    /// <param name="cqrsSaveMode">Numeric save mode used by generated CQRS write handlers.</param>
    /// <param name="profile">Numeric profile value used for starter defaults.</param>
    /// <example>
    /// <code>var config = new ArchitectureConfig(0, true, false, true, generateDependencyInjection: true);</code>
    /// </example>
    public ArchitectureConfig(
        int pattern, 
        bool useSpecification, 
        bool useUnitOfWork, 
        bool enableValidation,
        bool generateDependencyInjection = false,
        bool generateEndpoints = false,
        bool enableExperimentalEndpoints = false,
        bool generateDtos = false,
        bool generateEfConfigurations = false,
        bool generateCachingDecorators = false,
        bool generatePagination = false,
        string? cqrsDbContextTypeName = null,
        int cqrsSaveMode = 0,
        int profile = 0)
    {
        Profile = profile;
        Pattern = pattern;
        UseSpecification = useSpecification;
        UseUnitOfWork = useUnitOfWork;
        EnableValidation = enableValidation;
        GenerateDependencyInjection = generateDependencyInjection;
        GenerateEndpoints = generateEndpoints;
        EnableExperimentalEndpoints = enableExperimentalEndpoints;
        GenerateDtos = generateDtos;
        GenerateEfConfigurations = generateEfConfigurations;
        GenerateCachingDecorators = generateCachingDecorators;
        GeneratePagination = generatePagination;
        CqrsDbContextTypeName = string.IsNullOrWhiteSpace(cqrsDbContextTypeName)
            ? DefaultCqrsDbContextTypeName
            : cqrsDbContextTypeName!;
        CqrsSaveMode = cqrsSaveMode;
    }

    /// <summary>
    /// Default configuration: CQRS pattern with no optional features.
    /// Used internally before preflight diagnostics enforce explicit configuration.
    /// </summary>
    public static ArchitectureConfig Default { get; } = new(0, false, false, false);

    /// <summary>
    /// Gets a value indicating whether CQRS generation is active.
    /// </summary>
    public bool IsCqrs => Pattern == 0 || Pattern == 2;

    /// <summary>
    /// Gets a value indicating whether repository generation is active.
    /// </summary>
    public bool IsRepository => Pattern == 1 || Pattern == 2;

    /// <summary>
    /// Gets a value indicating whether per-request transaction save mode is active.
    /// </summary>
    public bool IsPerRequestTransactionSaveMode => CqrsSaveMode == 1;

    /// <summary>
    /// Determines whether this instance is equal to another configuration instance.
    /// </summary>
    /// <param name="other">The other configuration to compare.</param>
    /// <returns><see langword="true"/> when all effective values match; otherwise <see langword="false"/>.</returns>
    public bool Equals(ArchitectureConfig? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Pattern == other.Pattern
            && Profile == other.Profile
            && UseSpecification == other.UseSpecification
            && UseUnitOfWork == other.UseUnitOfWork
            && EnableValidation == other.EnableValidation
            && GenerateDependencyInjection == other.GenerateDependencyInjection
            && GenerateEndpoints == other.GenerateEndpoints
            && EnableExperimentalEndpoints == other.EnableExperimentalEndpoints
            && GenerateDtos == other.GenerateDtos
            && GenerateEfConfigurations == other.GenerateEfConfigurations
            && GenerateCachingDecorators == other.GenerateCachingDecorators
            && GeneratePagination == other.GeneratePagination
            && string.Equals(CqrsDbContextTypeName, other.CqrsDbContextTypeName, StringComparison.Ordinal)
            && CqrsSaveMode == other.CqrsSaveMode;
    }

    /// <summary>
    /// Determines whether this instance is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><see langword="true"/> when the object is a matching configuration; otherwise <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => Equals(obj as ArchitectureConfig);

    /// <summary>
    /// Computes a stable hash code for incremental cache invalidation.
    /// </summary>
    /// <returns>The hash code for this configuration.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = (int)2166136261;
            hash = (hash ^ Profile) * 16777619;
            hash = (hash ^ Pattern) * 16777619;
            hash = (hash ^ UseSpecification.GetHashCode()) * 16777619;
            hash = (hash ^ UseUnitOfWork.GetHashCode()) * 16777619;
            hash = (hash ^ EnableValidation.GetHashCode()) * 16777619;
            hash = (hash ^ GenerateDependencyInjection.GetHashCode()) * 16777619;
            hash = (hash ^ GenerateEndpoints.GetHashCode()) * 16777619;
            hash = (hash ^ EnableExperimentalEndpoints.GetHashCode()) * 16777619;
            hash = (hash ^ GenerateDtos.GetHashCode()) * 16777619;
            hash = (hash ^ GenerateEfConfigurations.GetHashCode()) * 16777619;
            hash = (hash ^ GenerateCachingDecorators.GetHashCode()) * 16777619;
            hash = (hash ^ GeneratePagination.GetHashCode()) * 16777619;
            hash = (hash ^ StringComparer.Ordinal.GetHashCode(CqrsDbContextTypeName)) * 16777619;
            hash = (hash ^ CqrsSaveMode.GetHashCode()) * 16777619;
            return hash;
        }
    }
}
