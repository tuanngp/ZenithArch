using System;

namespace RynorArch.Generator.Models;

/// <summary>
/// Architecture configuration extracted from the assembly-level [Architecture] attribute.
/// Determines which emitters are activated during code generation.
/// </summary>
public sealed class ArchitectureConfig : IEquatable<ArchitectureConfig>
{
    public const string DefaultCqrsDbContextTypeName = "global::Microsoft.EntityFrameworkCore.DbContext";

    /// <summary>0 = Cqrs, 1 = Repository, 2 = FullStack</summary>
    public int Pattern { get; }
    public bool UseSpecification { get; }
    public bool UseUnitOfWork { get; }
    public bool EnableValidation { get; }
    public bool GenerateDependencyInjection { get; }
    public bool GenerateEndpoints { get; }
    public bool EnableExperimentalEndpoints { get; }
    public bool GenerateDtos { get; }
    public bool GenerateEfConfigurations { get; }
    public bool GenerateCachingDecorators { get; }
    public bool GeneratePagination { get; }
    public string CqrsDbContextTypeName { get; }
    public int CqrsSaveMode { get; }

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
        int cqrsSaveMode = 0)
    {
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

    public bool IsCqrs => Pattern == 0 || Pattern == 2;
    public bool IsRepository => Pattern == 1 || Pattern == 2;
    public bool IsPerRequestTransactionSaveMode => CqrsSaveMode == 1;

    public bool Equals(ArchitectureConfig? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Pattern == other.Pattern
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

    public override bool Equals(object? obj) => Equals(obj as ArchitectureConfig);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = (int)2166136261;
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
