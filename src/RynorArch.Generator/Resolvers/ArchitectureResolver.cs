using Microsoft.CodeAnalysis;
using RynorArch.Generator.Diagnostics;
using RynorArch.Generator.Emitters;
using RynorArch.Generator.Models;

namespace RynorArch.Generator.Resolvers;

/// <summary>
/// Routes code generation to the appropriate emitters based on the architecture configuration.
/// Enforces strict pattern isolation: CQRS and Repository modes are mutually exclusive
/// unless FullStack is explicitly enabled.
/// </summary>
internal static class ArchitectureResolver
{
    /// <summary>
    /// Resolves which emitters to invoke for a given entity based on the architecture config.
    /// Reports diagnostics on pattern conflicts.
    /// </summary>
    public static void Resolve(SourceProductionContext context, EntityModel entity, ArchitectureConfig config)
    {
        // Pattern 0 = CQRS
        if (config.Pattern == 0)
        {
            CqrsEmitter.Emit(context, entity, config);

            if (config.UseUnitOfWork)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.PatternConflict,
                    Location.None,
                    "UseUnitOfWork is set to true but Pattern is CQRS. UnitOfWork is only generated for Repository or FullStack patterns."));
            }
        }
        // Pattern 1 = Repository
        else if (config.Pattern == 1)
        {
            RepositoryEmitter.Emit(context, entity, config);

            if (config.EnableValidation)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.PatternConflict,
                    Location.None,
                    "EnableValidation is set to true but Pattern is Repository. Validators are only generated for CQRS or FullStack patterns."));
            }
        }
        // Pattern 2 = FullStack
        else if (config.Pattern == 2)
        {
            CqrsEmitter.Emit(context, entity, config);
            RepositoryEmitter.Emit(context, entity, config);
        }

        // Specification — available for all patterns when enabled
        if (config.UseSpecification)
        {
            SpecificationEmitter.Emit(context, entity);
        }

        // Validation — only for patterns that generate commands
        if (config.EnableValidation && config.IsCqrs)
        {
            ValidationEmitter.Emit(context, entity);
        }

        // DDD — for aggregate roots regardless of pattern
        if (entity.IsAggregateRoot)
        {
            DddEmitter.Emit(context, entity);
        }
    }
}
