using Microsoft.CodeAnalysis;

namespace RynorArch.Generator.Diagnostics;

/// <summary>
/// Compile-time diagnostic descriptors for the RynorArch generator.
/// All IDs follow the RYNOR prefix convention.
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string Category = "RynorArch";

    public static readonly DiagnosticDescriptor NoEntitiesFound = new(
        id: "RYNOR001",
        title: "No entities found",
        messageFormat: "No classes with [Entity] attribute were found in the assembly. No code will be generated.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AggregateRootWithoutEntity = new(
        id: "RYNOR002",
        title: "AggregateRoot requires Entity",
        messageFormat: "Class '{0}' has [AggregateRoot] but is missing [Entity]. Add [Entity] attribute to enable code generation.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PatternConflict = new(
        id: "RYNOR003",
        title: "Architecture pattern conflict",
        messageFormat: "{0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnsupportedFilterType = new(
        id: "RYNOR004",
        title: "Unsupported QueryFilter type",
        messageFormat: "Property '{0}' on entity '{1}' has an unsupported type for [QueryFilter]. Supported types: string, numeric, bool, DateTime, Guid, and their nullable variants.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EntityMustBePartial = new(
        id: "RYNOR005",
        title: "Entity must be partial",
        messageFormat: "Class '{0}' has [Entity] but is not declared as partial. Generated code requires partial classes.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingArchitectureConfiguration = new(
        id: "RYNOR006",
        title: "Missing architecture configuration",
        messageFormat: "No [assembly: Architecture(...)] configuration was found. RynorArch does not generate code without explicit architecture configuration.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingRequiredDependency = new(
        id: "RYNOR007",
        title: "Missing required dependency",
        messageFormat: "Feature '{0}' requires dependency '{1}'. Add the dependency to the project or disable the feature flag.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidConfiguredDbContextType = new(
        id: "RYNOR008",
        title: "Configured DbContext type is invalid",
        messageFormat: "Configured DbContext type '{0}' must resolve in the compilation and derive from 'Microsoft.EntityFrameworkCore.DbContext'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EndpointBehaviorNotice = new(
        id: "RYNOR009",
        title: "Generated endpoint behavior notice",
        messageFormat: "Generated endpoints return generic HTTP responses and may need manual hardening for enterprise API semantics (for example 404 handling, problem details, and authorization)",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CachingBehaviorNotice = new(
        id: "RYNOR010",
        title: "Generated cache behavior notice",
        messageFormat: "Generated cache behavior stores query responses and emits per-entity invalidation contracts for write flows. Ensure invalidators are registered in DI.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor FeatureFlagIgnored = new(
        id: "RYNOR011",
        title: "Feature flag ignored by selected pattern",
        messageFormat: "{0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ExperimentalEndpointFlagRequired = new(
        id: "RYNOR012",
        title: "Endpoint generation requires experimental opt-in",
        messageFormat: "GenerateEndpoints is enabled, but endpoint generation is experimental. Set EnableExperimentalEndpoints = true to opt in explicitly.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
