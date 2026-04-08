using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using RynorArch.Generator.Helpers;
using RynorArch.Generator.Models;

namespace RynorArch.Generator.Emitters;

internal static class GenerationReportEmitter
{
    public static void Emit(SourceProductionContext context, ImmutableArray<EntityModel> entities, ArchitectureConfig config)
    {
        var w = new SourceWriter(4096);
        w.AppendFileHeader("Global.GenerationReport");
        w.AppendLine("namespace RynorArch.Generated.Diagnostics;");
        w.AppendLine();
        w.AppendLine("public static class RynorArchGenerationReport");
        w.OpenBrace();

        w.AppendLine($"public const int EntityCount = {entities.Length};");
        w.AppendLine($"public const int Profile = {config.Profile};");
        w.AppendLine($"public const int Pattern = {config.Pattern};");
        w.AppendLine($"public const bool UseSpecification = {config.UseSpecification.ToString().ToLowerInvariant()};");
        w.AppendLine($"public const bool UseUnitOfWork = {config.UseUnitOfWork.ToString().ToLowerInvariant()};");
        w.AppendLine($"public const bool EnableValidation = {config.EnableValidation.ToString().ToLowerInvariant()};");
        w.AppendLine($"public const bool GenerateDependencyInjection = {config.GenerateDependencyInjection.ToString().ToLowerInvariant()};");
        w.AppendLine($"public const bool GenerateEndpoints = {config.GenerateEndpoints.ToString().ToLowerInvariant()};");
        w.AppendLine($"public const bool EnableExperimentalEndpoints = {config.EnableExperimentalEndpoints.ToString().ToLowerInvariant()};");
        w.AppendLine($"public const bool GenerateDtos = {config.GenerateDtos.ToString().ToLowerInvariant()};");
        w.AppendLine($"public const bool GenerateEfConfigurations = {config.GenerateEfConfigurations.ToString().ToLowerInvariant()};");
        w.AppendLine($"public const bool GenerateCachingDecorators = {config.GenerateCachingDecorators.ToString().ToLowerInvariant()};");
        w.AppendLine($"public const bool GeneratePagination = {config.GeneratePagination.ToString().ToLowerInvariant()};");
        w.AppendLine($"public const int CqrsSaveMode = {config.CqrsSaveMode};");
        w.AppendLine($"public const string CqrsDbContextTypeName = \"{config.CqrsDbContextTypeName}\";");
        w.AppendLine();

        w.AppendLine("public static readonly string[] Entities = new[]");
        w.OpenBrace();
        foreach (var entity in GetEntitiesSorted(entities))
        {
            w.AppendLine($"\"{entity.Name}\",");
        }
        w.CloseBrace(semicolon: true);
        w.AppendLine();

        w.AppendLine("public static readonly string[] Artifacts = new[]");
        w.OpenBrace();
        foreach (var artifact in BuildArtifactList(entities, config))
        {
            w.AppendLine($"\"{artifact}\",");
        }
        w.CloseBrace(semicolon: true);

        w.CloseBrace();
        context.AddSource("RynorArch.GenerationReport.g.cs", w.ToString());
    }

    private static IEnumerable<string> BuildArtifactList(ImmutableArray<EntityModel> entities, ArchitectureConfig config)
    {
        var list = new List<string>();
        if (entities.Length > 0 && (config.IsCqrs || config.IsRepository))
        {
            list.Add("RynorArch.CrudInfrastructure.g.cs");
        }

        if (entities.Length > 0 && config.IsCqrs && config.IsPerRequestTransactionSaveMode)
        {
            list.Add("RynorArch.CqrsSaveBehavior.g.cs");
        }

        if (entities.Length > 0 && config.IsCqrs && config.EnableValidation)
        {
            list.Add("RynorArch.ValidationBehavior.g.cs");
        }

        if (entities.Length > 0 && config.IsRepository && config.UseUnitOfWork)
        {
            list.Add("IUnitOfWork.g.cs");
        }

        if (config.GenerateDependencyInjection)
        {
            list.Add("RynorArchServiceCollectionExtensions.g.cs");
        }

        if (config.GenerateEndpoints && config.EnableExperimentalEndpoints && config.IsCqrs)
        {
            list.Add("RynorArchEndpointExtensions.g.cs");
        }

        foreach (var entity in GetEntitiesSorted(entities))
        {
            if (config.IsCqrs) list.Add($"{entity.Name}.Cqrs.g.cs");
            if (config.IsRepository) list.Add($"{entity.Name}.Repository.g.cs");
            if (config.UseSpecification) list.Add($"{entity.Name}.Specification.g.cs");
            if (config.EnableValidation && config.IsCqrs) list.Add($"{entity.Name}.Validation.g.cs");
            if (config.GenerateDtos) list.Add($"{entity.Name}Dto.g.cs");
            if (config.GenerateEfConfigurations) list.Add($"{entity.Name}Configuration.g.cs");
            if (config.GenerateCachingDecorators && config.IsCqrs) list.Add($"Get{entity.Name}ByIdQueryCacheBehavior.g.cs");
            if (config.GeneratePagination) list.Add($"{entity.Name}PaginationExtensions.g.cs");
            if (entity.IsAggregateRoot) list.Add($"{entity.Name}.DomainEvents.g.cs");
        }

        list.Sort(System.StringComparer.Ordinal);
        return list;
    }

    private static List<EntityModel> GetEntitiesSorted(ImmutableArray<EntityModel> entities)
    {
        var list = new List<EntityModel>(entities.Length);
        for (int i = 0; i < entities.Length; i++)
        {
            list.Add(entities[i]);
        }

        list.Sort(EntityNameComparer.Instance);
        return list;
    }

    private sealed class EntityNameComparer : IComparer<EntityModel>
    {
        public static EntityNameComparer Instance { get; } = new();
        public int Compare(EntityModel? x, EntityModel? y) =>
            System.StringComparer.Ordinal.Compare(x?.Name, y?.Name);
    }
}
