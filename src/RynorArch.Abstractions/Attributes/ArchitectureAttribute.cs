using RynorArch.Abstractions.Enums;

namespace RynorArch.Abstractions.Attributes;

/// <summary>
/// Assembly-level attribute that configures the RynorArch source generator behavior.
/// Controls which architecture pattern and features are enabled for code generation.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
public sealed class ArchitectureAttribute : Attribute
{
    /// <summary>
    /// The architecture pattern to enforce. Determines which code artifacts are generated.
    /// </summary>
    public ArchitecturePattern Pattern { get; set; } = ArchitecturePattern.Cqrs;

    /// <summary>
    /// When true, generates ISpecification&lt;T&gt; implementations from [QueryFilter] properties.
    /// </summary>
    public bool UseSpecification { get; set; }

    /// <summary>
    /// When true, generates IUnitOfWork interface with lazy-loaded repositories.
    /// Only applicable when Pattern is Repository or FullStack.
    /// </summary>
    public bool UseUnitOfWork { get; set; }

    /// <summary>
    /// When true, generates FluentValidation validator stubs for commands.
    /// Only applicable when Pattern is Cqrs or FullStack.
    /// </summary>
    public bool EnableValidation { get; set; }
}
