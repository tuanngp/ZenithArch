using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using RynorArch.Generator.Emitters;
using RynorArch.Generator.Models;

namespace RynorArch.Generator.Resolvers;

internal static class GlobalResolver
{
    public static void Resolve(SourceProductionContext context, ImmutableArray<EntityModel> entities, ArchitectureConfig config)
    {
        if (entities.Length > 0 && (config.IsCqrs || config.IsRepository))
        {
            CrudInfrastructureEmitter.Emit(context);
        }

        if (entities.Length > 0 && config.IsRepository && config.UseUnitOfWork)
        {
            context.AddSource("IUnitOfWork.g.cs", RepositoryEmitter.GenerateUnitOfWorkInterface());
        }

        if (config.GenerateDependencyInjection)
        {
            DependencyInjectionEmitter.Emit(context, entities, config);
        }

        if (config.GenerateEndpoints && config.IsCqrs)
        {
            EndpointEmitter.Emit(context, entities, config);
        }
    }
}
