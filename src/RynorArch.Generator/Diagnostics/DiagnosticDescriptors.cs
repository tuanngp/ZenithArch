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
        messageFormat: "No [assembly: Architecture(...)] configuration was found. RynorArch will fall back to default CQRS settings. Add an explicit assembly configuration to keep generation predictable.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
