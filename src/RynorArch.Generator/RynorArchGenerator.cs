using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RynorArch.Generator.Diagnostics;
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
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Step 1: Find all [Entity] classes using the most efficient Roslyn API.
        // ForAttributeWithMetadataName uses a syntax-level index — O(1) discovery.
        IncrementalValuesProvider<EntityModel?> entityProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "RynorArch.Abstractions.Attributes.EntityAttribute",
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) => EntityTransformer.TransformEntity(ctx, ct));

        // Filter out nulls
        IncrementalValuesProvider<EntityModel> validEntities = entityProvider
            .Where(static entity => entity is not null)!;

        // Step 2: Extract architecture configuration from assembly-level attribute.
        // Uses CompilationProvider — invalidates only when compilation changes structurally.
        IncrementalValueProvider<ArchitectureConfig> configProvider = context.CompilationProvider
            .Select(static (compilation, ct) => EntityTransformer.ExtractConfig(compilation, ct));

        // Step 3: Combine each entity with the architecture config.
        IncrementalValuesProvider<(EntityModel Entity, ArchitectureConfig Config)> combined =
            validEntities.Combine(configProvider);

        // Step 4: Emit code — each entity is processed independently, enabling parallelism.
        context.RegisterSourceOutput(combined, static (spc, pair) =>
        {
            ArchitectureResolver.Resolve(spc, pair.Entity, pair.Config);
        });

        // Step 5: Diagnostic — warn if no entities found.
        IncrementalValueProvider<(ImmutableArray<EntityModel> Entities, ArchitectureConfig Config)> collectedForDiagnostics =
            validEntities.Collect().Combine(configProvider);

        context.RegisterSourceOutput(collectedForDiagnostics, static (spc, pair) =>
        {
            if (pair.Entities.Length == 0)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.NoEntitiesFound,
                    Location.None));
            }
        });

        // Step 6: AggregateRoot validation — check [AggregateRoot] without [Entity].
        // Uses a separate provider to detect misuse.
        IncrementalValuesProvider<string?> aggregateRootWithoutEntity = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "RynorArch.Abstractions.Attributes.AggregateRootAttribute",
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) =>
                {
                    if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol)
                        return null;

                    // Check if it also has [Entity]
                    var attributes = typeSymbol.GetAttributes();
                    for (int i = 0; i < attributes.Length; i++)
                    {
                        if (attributes[i].AttributeClass?.ToDisplayString() == "RynorArch.Abstractions.Attributes.EntityAttribute")
                            return null; // Valid — has both attributes
                    }

                    return typeSymbol.Name; // Invalid — AggregateRoot without Entity
                })
            .Where(static name => name is not null);

        context.RegisterSourceOutput(aggregateRootWithoutEntity, static (spc, className) =>
        {
            spc.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.AggregateRootWithoutEntity,
                Location.None,
                className));
        });
    }
}
