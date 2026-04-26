using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ZenithArch.Generator.Diagnostics;
using ZenithArch.Generator.Emitters;
using ZenithArch.Generator.Helpers;
using ZenithArch.Generator.Models;

namespace ZenithArch.Generator.Resolvers;

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
            CrudInfrastructureEmitter.Emit(context, config.IsRepository && config.UseUnitOfWork);
        }

        if (entities.Length > 0 && config.IsCqrs && config.IsPerRequestTransactionSaveMode)
        {
            CqrsSaveBehaviorEmitter.Emit(context, config.CqrsDbContextTypeName);
        }

        if (entities.Length > 0 && config.IsCqrs && config.EnableValidation)
        {
            ValidationPipelineBehaviorEmitter.Emit(context);
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

        bool hasMediator = HasType(compilation, "MediatR.IMediator");
        bool hasFluentValidation = HasType(compilation, "FluentValidation.AbstractValidator`1");
        var dbContextSymbol = compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbContext");
        bool hasDbContext = dbContextSymbol is not null;
        bool hasEndpointRouteBuilder = HasType(compilation, "Microsoft.AspNetCore.Routing.IEndpointRouteBuilder");
        bool hasDistributedCache = HasType(compilation, "Microsoft.Extensions.Caching.Distributed.IDistributedCache");

        if (config.IsCqrs && !hasMediator)
        {
            ReportMissingDependency(
                context,
                location,
                "CQRS/FullStack",
                "MediatR",
                "<PackageReference Include=\"MediatR\" Version=\"14.*\" />");
        }

        if (config.EnableValidation && !hasFluentValidation)
        {
            ReportMissingDependency(
                context,
                location,
                "EnableValidation",
                "FluentValidation",
                "<PackageReference Include=\"FluentValidation\" Version=\"12.*\" />");
        }

        if ((config.IsCqrs || config.IsRepository || config.GenerateEfConfigurations) &&
            !hasDbContext)
        {
            ReportMissingDependency(
                context,
                location,
                "Persistence features",
                "Microsoft.EntityFrameworkCore",
                "<PackageReference Include=\"Microsoft.EntityFrameworkCore\" Version=\"10.*\" />");
        }

        if (config.IsCqrs
            && hasDbContext
            && !ValidateConfiguredDbContextType(compilation, config.CqrsDbContextTypeName, dbContextSymbol!))
        {
            DiagnosticReporter.Report(
                context,
                DiagnosticDescriptors.InvalidConfiguredDbContextType,
                location,
                config.CqrsDbContextTypeName);
        }

        if (config.GenerateEndpoints && !config.EnableExperimentalEndpoints)
        {
            DiagnosticReporter.Report(
                context,
                DiagnosticDescriptors.ExperimentalEndpointFlagRequired,
                location);
        }
        else if (config.GenerateEndpoints && !hasEndpointRouteBuilder)
        {
            ReportMissingDependency(
                context,
                location,
                "GenerateEndpoints",
                "Microsoft.AspNetCore.App",
                "<FrameworkReference Include=\"Microsoft.AspNetCore.App\" />");
        }
        else if (config.GenerateEndpoints && !config.IsCqrs)
        {
            DiagnosticReporter.Report(
                context,
                DiagnosticDescriptors.FeatureFlagIgnored,
                location,
                "GenerateEndpoints is set to true but selected pattern does not generate CQRS request/handler types.");
        }

        if (config.EndpointHardeningMode != 0)
        {
            if (!config.GenerateEndpoints)
            {
                DiagnosticReporter.Report(
                    context,
                    DiagnosticDescriptors.InvalidEndpointHardeningConfiguration,
                    location,
                    "EndpointHardeningMode is configured while GenerateEndpoints is false. Enable GenerateEndpoints and EnableExperimentalEndpoints, or reset EndpointHardeningMode to None.");
            }
            else if (!config.EnableExperimentalEndpoints)
            {
                DiagnosticReporter.Report(
                    context,
                    DiagnosticDescriptors.InvalidEndpointHardeningConfiguration,
                    location,
                    "EndpointHardeningMode requires EnableExperimentalEndpoints = true when endpoint generation is enabled.");
            }
            else if (!config.IsCqrs)
            {
                DiagnosticReporter.Report(
                    context,
                    DiagnosticDescriptors.InvalidEndpointHardeningConfiguration,
                    location,
                    "EndpointHardeningMode is configured but selected pattern does not generate CQRS endpoints.");
            }
        }

        if (config.GenerateCachingDecorators && !hasDistributedCache)
        {
            ReportMissingDependency(
                context,
                location,
                "GenerateCachingDecorators",
                "Microsoft.Extensions.Caching.*",
                "<PackageReference Include=\"Microsoft.Extensions.Caching.StackExchangeRedis\" Version=\"10.*\" />");
        }

        if (config.IsPerRequestTransactionSaveMode && !config.GenerateDependencyInjection)
        {
            DiagnosticReporter.Report(
                context,
                DiagnosticDescriptors.SaveModeRequiresGeneratedDi,
                location);
        }

        if (config.EnableValidation && !config.GenerateDependencyInjection)
        {
            DiagnosticReporter.Report(
                context,
                DiagnosticDescriptors.ValidationRequiresGeneratedDi,
                location);
        }

        if (config.GenerateEndpoints && config.EnableExperimentalEndpoints && config.IsCqrs)
        {
            if (config.EndpointHardeningMode == 0)
            {
                DiagnosticReporter.Report(
                    context,
                    DiagnosticDescriptors.EndpointBehaviorNotice,
                    location);
            }

            DiagnosticReporter.Report(
                context,
                DiagnosticDescriptors.EndpointHardeningChecklistRequired,
                location);
        }

        if (config.GenerateCachingDecorators && config.IsCqrs)
        {
            DiagnosticReporter.Report(
                context,
                DiagnosticDescriptors.CachingBehaviorNotice,
                location);
        }

        if (ShouldSuggestProfileMigration(config))
        {
            DiagnosticReporter.Report(
                context,
                DiagnosticDescriptors.LegacyConfigurationProfileHint,
                location,
                GetSuggestedProfile(config));
        }
    }

    private static void ReportMissingDependency(
        SourceProductionContext context,
        Location location,
        string feature,
        string packageName,
        string referenceHint)
    {
        DiagnosticReporter.Report(
            context,
            DiagnosticDescriptors.MissingRequiredDependency,
            location,
            feature,
            packageName,
            referenceHint);
    }

    private static bool HasType(Compilation compilation, string metadataName) =>
        compilation.GetTypeByMetadataName(metadataName) is not null;

    private static bool ValidateConfiguredDbContextType(Compilation compilation, string configuredTypeName, INamedTypeSymbol dbContextSymbol)
    {
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

    private static bool ShouldSuggestProfileMigration(ArchitectureConfig config)
    {
        if (config.Profile != 0)
        {
            return false;
        }

        return config.UseSpecification
            || config.UseUnitOfWork
            || config.EnableValidation
            || config.GenerateDependencyInjection
            || config.GenerateEndpoints
            || config.EnableExperimentalEndpoints
            || config.EndpointHardeningMode != 0
            || config.GenerateDtos
            || config.GenerateEfConfigurations
            || config.GenerateCachingDecorators
            || config.GeneratePagination
            || config.CqrsSaveMode != 0;
    }

    private static string GetSuggestedProfile(ArchitectureConfig config)
    {
        if (config.Pattern == 1)
        {
            return "RepositoryQuickStart";
        }

        if (config.Pattern == 2)
        {
            return "FullStackQuickStart";
        }

        return "CqrsQuickStart";
    }
}
