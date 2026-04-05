using System;

namespace RynorArch.Generator.Models;

/// <summary>
/// Architecture configuration extracted from the assembly-level [Architecture] attribute.
/// Determines which emitters are activated during code generation.
/// </summary>
public sealed class ArchitectureConfig : IEquatable<ArchitectureConfig>
{
    /// <summary>0 = Cqrs, 1 = Repository, 2 = FullStack</summary>
    public int Pattern { get; }
    public bool UseSpecification { get; }
    public bool UseUnitOfWork { get; }
    public bool EnableValidation { get; }

    public ArchitectureConfig(int pattern, bool useSpecification, bool useUnitOfWork, bool enableValidation)
    {
        Pattern = pattern;
        UseSpecification = useSpecification;
        UseUnitOfWork = useUnitOfWork;
        EnableValidation = enableValidation;
    }

    /// <summary>
    /// Default configuration: CQRS pattern with no optional features.
    /// Used when no [assembly: Architecture] attribute is found.
    /// </summary>
    public static ArchitectureConfig Default { get; } = new(0, false, false, false);

    public bool IsCqrs => Pattern == 0 || Pattern == 2;
    public bool IsRepository => Pattern == 1 || Pattern == 2;

    public bool Equals(ArchitectureConfig? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Pattern == other.Pattern
            && UseSpecification == other.UseSpecification
            && UseUnitOfWork == other.UseUnitOfWork
            && EnableValidation == other.EnableValidation;
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
            return hash;
        }
    }
}
