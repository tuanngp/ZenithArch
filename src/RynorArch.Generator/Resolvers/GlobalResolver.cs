using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using RynorArch.Generator.Diagnostics;
using RynorArch.Generator.Emitters;
using RynorArch.Generator.Models;

namespace RynorArch.Generator.Resolvers;

internal static class GlobalResolver
{
    public static void Resolve(
        SourceProductionContext context,
        ImmutableArray<EntityModel> entities,
        ArchitectureConfig config,
        Compilation compilation,
        Location diagnosticLocation)
    {
        EmitPreflightDiagnostics(context, entities, config, compilation, diagnosticLocation);

        if (entities.Length > 0 && (config.IsCqrs || config.IsRepository))
        {
            CrudInfrastructureEmitter.Emit(context);
        }

        if (entities.Length > 0 && config.IsCqrs && config.IsPerRequestTransactionSaveMode)
        {
            CqrsSaveBehaviorEmitter.Emit(context, config.CqrsDbContextTypeName);
        }

        if (entities.Length > 0 && config.IsRepository && config.UseUnitOfWork)
        {
            context.AddSource("IUnitOfWork.g.cs", RepositoryEmitter.GenerateUnitOfWorkInterface());
        }

        if (config.GenerateDependencyInjection)
        {
            DependencyInjectionEmitter.Emit(context, entities, config);
        }

        if (config.GenerateEndpoints && config.EnableExperimentalEndpoints && config.IsCqrs)
        {
            EndpointEmitter.Emit(context, entities, config);
        }

        GenerationReportEmitter.Emit(context, entities, config);
    }

    private static void EmitPreflightDiagnostics(
        SourceProductionContext context,
        ImmutableArray<EntityModel> entities,
        ArchitectureConfig config,
        Compilation compilation,
        Location location)
    {
        if (entities.Length == 0)
        {
            return;
        }

        if (config.IsCqrs && !HasType(compilation, "MediatR.IMediator"))
        {
            ReportMissingDependency(context, location, "CQRS/FullStack", "MediatR");
        }

        if (config.EnableValidation && !HasType(compilation, "FluentValidation.AbstractValidator`1"))
        {
            ReportMissingDependency(context, location, "EnableValidation", "FluentValidation");
        }

        if ((config.IsCqrs || config.IsRepository || config.GenerateEfConfigurations) &&
            !HasType(compilation, "Microsoft.EntityFrameworkCore.DbContext"))
        {
            ReportMissingDependency(context, location, "Persistence features", "Microsoft.EntityFrameworkCore");
        }

        if (config.IsCqrs
            && HasType(compilation, "Microsoft.EntityFrameworkCore.DbContext")
            && !ValidateConfiguredDbContextType(compilation, config.CqrsDbContextTypeName))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.InvalidConfiguredDbContextType,
                location,
                config.CqrsDbContextTypeName));
        }

        if (config.GenerateEndpoints && !config.EnableExperimentalEndpoints)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.ExperimentalEndpointFlagRequired,
                location));
        }
        else if (config.GenerateEndpoints && !HasType(compilation, "Microsoft.AspNetCore.Routing.IEndpointRouteBuilder"))
        {
            ReportMissingDependency(context, location, "GenerateEndpoints", "Microsoft.AspNetCore.App");
        }
        else if (config.GenerateEndpoints && !config.IsCqrs)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.FeatureFlagIgnored,
                location,
                "GenerateEndpoints is set to true but selected pattern does not generate CQRS request/handler types."));
        }

        if (config.GenerateCachingDecorators && !HasType(compilation, "Microsoft.Extensions.Caching.Distributed.IDistributedCache"))
        {
            ReportMissingDependency(context, location, "GenerateCachingDecorators", "Microsoft.Extensions.Caching.*");
        }

        if (config.GenerateEndpoints && config.EnableExperimentalEndpoints && config.IsCqrs)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.EndpointBehaviorNotice,
                location));
        }

        if (config.GenerateCachingDecorators && config.IsCqrs)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.CachingBehaviorNotice,
                location));
        }
    }

    private static void ReportMissingDependency(SourceProductionContext context, Location location, string feature, string packageName)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.MissingRequiredDependency,
            location,
            feature,
            packageName));
    }

    private static bool HasType(Compilation compilation, string metadataName) =>
        compilation.GetTypeByMetadataName(metadataName) is not null;

    private static bool ValidateConfiguredDbContextType(Compilation compilation, string configuredTypeName)
    {
        var dbContextSymbol = compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbContext");
        if (dbContextSymbol is null)
        {
            return false;
        }

        var resolved = compilation.GetTypeByMetadataName(ToMetadataName(configuredTypeName));
        if (resolved is null)
        {
            return false;
        }

        if (SymbolEqualityComparer.Default.Equals(resolved, dbContextSymbol))
        {
            return true;
        }

        for (INamedTypeSymbol? current = resolved.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, dbContextSymbol))
            {
                return true;
            }
        }

        return false;
    }

    private static string ToMetadataName(string configuredTypeName)
    {
        const string globalPrefix = "global::";
        return configuredTypeName.StartsWith(globalPrefix, System.StringComparison.Ordinal)
            ? configuredTypeName.Substring(globalPrefix.Length)
            : configuredTypeName;
    }
}
