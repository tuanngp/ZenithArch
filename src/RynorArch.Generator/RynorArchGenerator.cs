using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RynorArch.Generator.Diagnostics;
using RynorArch.Generator.Helpers;
using RynorArch.Generator.Models;
using RynorArch.Generator.Pipeline;
using RynorArch.Generator.Resolvers;

namespace RynorArch.Generator;

/// <summary>
/// RynorArch Incremental Source Generator.
/// 
/// Pipeline:
/// 1. ForAttributeWithMetadataName discovers all [Entity] classes (O(1) per keystroke)
/// 2. Semantic transformation builds EntityModel (cached via value equality)
/// 3. CompilationProvider extracts ArchitectureConfig from assembly attribute
/// 4. Combined provider routes to ArchitectureResolver for pattern-aware emission
/// 
/// Performance characteristics:
/// - No full compilation scan
/// - No LINQ in transform hot paths
/// - EquatableArray for deterministic cache invalidation
/// - Only re-generates when entity or config changes
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class RynorArchGenerator : IIncrementalGenerator
{
    private const string EntityAttributeFqn = "RynorArch.Abstractions.Attributes.EntityAttribute";
    private const string AggregateRootAttributeFqn = "RynorArch.Abstractions.Attributes.AggregateRootAttribute";
    private const string QueryFilterAttributeFqn = "RynorArch.Abstractions.Attributes.QueryFilterAttribute";

    /// <summary>
    /// Configures incremental pipelines for entity discovery, configuration resolution,
    /// diagnostics, and source emission.
    /// </summary>
    /// <param name="context">The Roslyn incremental generator initialization context.</param>
    /// <example>
    /// <code>// Invoked by Roslyn during compilation.
    /// // No direct user call is required.
    /// </code>
    /// </example>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Step 1: Find all [Entity] classes using the most efficient Roslyn API.
        // ForAttributeWithMetadataName uses a syntax-level index — O(1) discovery.
        IncrementalValuesProvider<bool> entityDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: EntityAttributeFqn,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (_, _) => true);

        IncrementalValuesProvider<EntityModel?> entityProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: EntityAttributeFqn,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) => EntityTransformer.TransformEntity(ctx, ct));

        // Filter out nulls
        IncrementalValuesProvider<EntityModel> validEntities = entityProvider
            .Where(static entity => entity is not null)!;

        IncrementalValuesProvider<(string ClassName, Location Location)?> nonPartialEntityDiagnostics = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: EntityAttributeFqn,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) =>
                {
                    ct.ThrowIfCancellationRequested();

                    if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol
                        || ctx.TargetNode is not ClassDeclarationSyntax classDeclaration
                        || EntityTransformer.IsPartialDeclaration(classDeclaration))
                    {
                        return ((string ClassName, Location Location)?)null;
                    }

                    return (typeSymbol.Name, classDeclaration.Identifier.GetLocation());
                })
            .Where(static entry => entry is not null);

        // Step 2: Extract architecture configuration from assembly-level attribute.
        // Uses CompilationProvider — invalidates only when compilation changes structurally.
        IncrementalValueProvider<ArchitectureConfig> configProvider = context.CompilationProvider
            .Select(static (compilation, ct) => EntityTransformer.ExtractConfig(compilation, ct));

        IncrementalValueProvider<bool> hasArchitectureConfiguration = context.CompilationProvider
            .Select(static (compilation, ct) => EntityTransformer.HasArchitectureConfiguration(compilation, ct));

        // Step 3: Combine each entity with the architecture config.
        IncrementalValuesProvider<((EntityModel Entity, ArchitectureConfig Config) Data, bool HasConfiguration)> combined =
            validEntities.Combine(configProvider).Combine(hasArchitectureConfiguration);

        // Step 4: Emit code — each entity is processed independently, enabling parallelism.
        context.RegisterSourceOutput(combined, static (spc, pair) =>
        {
            if (!pair.HasConfiguration)
            {
                return;
            }

            ArchitectureResolver.Resolve(spc, pair.Data.Entity, pair.Data.Config);
        });

        // Step 4.5: Global (Assembly-wide) code generation
        IncrementalValueProvider<(((ImmutableArray<EntityModel> Entities, ArchitectureConfig Config) Data, Compilation Compilation) Payload, bool HasConfiguration)> collectedForGlobal =
            validEntities.Collect().Combine(configProvider).Combine(context.CompilationProvider).Combine(hasArchitectureConfiguration);

        context.RegisterSourceOutput(collectedForGlobal, static (spc, pair) =>
        {
            if (!pair.HasConfiguration)
            {
                return;
            }

            var architectureLocation = GetArchitectureLocation(pair.Payload.Compilation) ?? Location.None;
            GlobalResolver.Resolve(spc, pair.Payload.Data.Entities, pair.Payload.Data.Config, pair.Payload.Compilation, architectureLocation);
        });

        // Step 5: Diagnostic — warn if no entities found.
        IncrementalValueProvider<(ImmutableArray<bool> EntityDeclarations, bool HasConfiguration)> entityAudit =
            entityDeclarations.Collect().Combine(hasArchitectureConfiguration);

        context.RegisterSourceOutput(entityAudit, static (spc, pair) =>
        {
            if (pair.EntityDeclarations.Length == 0)
            {
                DiagnosticReporter.Report(
                    spc,
                    DiagnosticDescriptors.NoEntitiesFound,
                    Location.None);
            }

            if (!pair.HasConfiguration)
            {
                DiagnosticReporter.Report(
                    spc,
                    DiagnosticDescriptors.MissingArchitectureConfiguration,
                    Location.None);
            }
        });

        context.RegisterSourceOutput(nonPartialEntityDiagnostics, static (spc, entry) =>
        {
            DiagnosticReporter.Report(
                spc,
                DiagnosticDescriptors.EntityMustBePartial,
                entry!.Value.Location,
                entry.Value.ClassName);
        });

        IncrementalValuesProvider<(string PropertyName, string EntityName, Location Location)?> unsupportedQueryFilters = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: QueryFilterAttributeFqn,
                predicate: static (node, _) => node is PropertyDeclarationSyntax,
                transform: static (ctx, ct) =>
                {
                    ct.ThrowIfCancellationRequested();

                    if (ctx.TargetSymbol is not IPropertySymbol propertySymbol)
                        return ((string PropertyName, string EntityName, Location Location)?)null;

                    if (EntityTransformer.IsSupportedQueryFilterType(propertySymbol.Type))
                        return ((string PropertyName, string EntityName, Location Location)?)null;

                    var location = propertySymbol.Locations.Length > 0 ? propertySymbol.Locations[0] : Location.None;
                    return ((string PropertyName, string EntityName, Location Location)?)(propertySymbol.Name, propertySymbol.ContainingType.Name, location);
                })
            .Where(static entry => entry is not null);

        context.RegisterSourceOutput(unsupportedQueryFilters, static (spc, entry) =>
        {
            DiagnosticReporter.Report(
                spc,
                DiagnosticDescriptors.UnsupportedFilterType,
                entry!.Value.Location,
                entry!.Value.PropertyName,
                entry.Value.EntityName);
        });

        // Step 6: AggregateRoot validation — check [AggregateRoot] without [Entity].
        // Uses a separate provider to detect misuse.
        IncrementalValuesProvider<(string ClassName, Location Location)?> aggregateRootWithoutEntity = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: AggregateRootAttributeFqn,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) =>
                {
                    if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol)
                        return ((string ClassName, Location Location)?)null;

                    // Check if it also has [Entity]
                    var attributes = typeSymbol.GetAttributes();
                    for (int i = 0; i < attributes.Length; i++)
                    {
                        if (attributes[i].AttributeClass?.ToDisplayString() == EntityAttributeFqn)
                            return ((string ClassName, Location Location)?)null; // Valid — has both attributes
                    }

                    var location = ctx.TargetNode is ClassDeclarationSyntax cls
                        ? cls.Identifier.GetLocation()
                        : typeSymbol.Locations[0];
                    return (typeSymbol.Name, location); // Invalid — AggregateRoot without Entity
                })
            .Where(static entry => entry is not null);

        context.RegisterSourceOutput(aggregateRootWithoutEntity, static (spc, entry) =>
        {
            DiagnosticReporter.Report(
                spc,
                DiagnosticDescriptors.AggregateRootWithoutEntity,
                entry!.Value.Location,
                entry.Value.ClassName);
        });
    }

    private static Location? GetArchitectureLocation(Compilation compilation)
    {
        foreach (var attr in compilation.Assembly.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() != "RynorArch.Abstractions.Attributes.ArchitectureAttribute")
            {
                continue;
            }

            return attr.ApplicationSyntaxReference?.GetSyntax().GetLocation();
        }

        return null;
    }
}
