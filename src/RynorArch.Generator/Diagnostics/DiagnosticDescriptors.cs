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
        messageFormat: "No [assembly: Architecture(...)] configuration was found; create AssemblyConfig.cs and add [assembly: Architecture(Pattern = ArchitecturePattern.Cqrs)]",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingRequiredDependency = new(
        id: "RYNOR007",
        title: "Missing required dependency",
        messageFormat: "Feature '{0}' requires dependency '{1}'; add {2} or disable the related feature flag",
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
        messageFormat: "Generated endpoints return generic HTTP responses and may need manual hardening for enterprise API semantics (for example 404 handling, problem details, and authorization). See docs/ENDPOINT_HARDENING.md.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CachingBehaviorNotice = new(
        id: "RYNOR010",
        title: "Generated cache behavior notice",
        messageFormat: "Generated cache behavior stores query responses and emits per-entity invalidation contracts; call AddRynorArchDependencies to register invalidators and cache pipeline behaviors. See docs/CACHING_OPERATIONS.md.",
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

    public static readonly DiagnosticDescriptor SaveModeRequiresGeneratedDi = new(
        id: "RYNOR013",
        title: "CQRS save mode needs generated DI wiring",
        messageFormat: "CqrsSaveMode.PerRequestTransaction is enabled while GenerateDependencyInjection is false; either enable generated DI or manually register IPipelineBehavior<,> with RynorArchSaveChangesBehavior<,>",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor LegacyConfigurationProfileHint = new(
        id: "RYNOR014",
        title: "Consider starter profile migration",
        messageFormat: "Configuration uses explicit legacy-style flags. Consider `Profile = ArchitectureProfile.{0}` to reduce setup drift. See docs/UPGRADING_PROFILES.md.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ValidationRequiresGeneratedDi = new(
        id: "RYNOR015",
        title: "Validation needs generated DI wiring",
        messageFormat: "EnableValidation is enabled while GenerateDependencyInjection is false; either enable generated DI or manually register IPipelineBehavior<,> with RynorArchValidationBehavior<,>",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EndpointHardeningChecklistRequired = new(
        id: "RYNOR016",
        title: "Endpoint hardening checklist recommended",
        messageFormat: "GenerateEndpoints is enabled. Before production rollout, apply hardening for authorization, problem details, observability, and API lifecycle. See docs/ENDPOINT_HARDENING.md.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);
}
