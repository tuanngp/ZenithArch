using Microsoft.CodeAnalysis;
using RynorArch.Generator.Helpers;
using RynorArch.Generator.Models;

namespace RynorArch.Generator.Emitters;

/// <summary>
/// Generates CQRS artifacts: Commands, Queries, and Handlers using MediatR.
/// All handlers are partial classes with lifecycle hooks.
/// </summary>
internal static class CqrsEmitter
{
    public static void Emit(SourceProductionContext context, EntityModel entity, ArchitectureConfig config)
    {
        var source = Generate(entity, config);
        context.AddSource($"{entity.Name}.Cqrs.g.cs", source);
    }

    internal static string Generate(EntityModel entity, ArchitectureConfig config)
    {
        var w = new SourceWriter(4096);
        w.AppendFileHeader();

        w.AppendLine("using System;");
        w.AppendLine("using System.Collections.Generic;");
        w.AppendLine("using System.Linq;");
        w.AppendLine("using MediatR;");
        w.AppendLine("using Microsoft.EntityFrameworkCore;");
        w.AppendLine("using RynorArch.Generated.Infrastructure;");
        w.AppendLine("using System.Threading;");
        w.AppendLine("using System.Threading.Tasks;");
        if (!string.IsNullOrEmpty(entity.Namespace))
        {
            w.AppendLine($"using {entity.Namespace};");
        }
        w.AppendLine();

        if (!string.IsNullOrEmpty(entity.Namespace))
        {
            w.AppendLine($"namespace {entity.Namespace}.Cqrs;");
            w.AppendLine();
        }

        EmitCommands(w, entity);
        EmitQueries(w, entity);
        EmitCreateHandler(w, entity);
        EmitUpdateHandler(w, entity);
        EmitDeleteHandler(w, entity);
        EmitGetByIdHandler(w, entity);
        EmitGetListHandler(w, entity);

        return w.ToString();
    }

    private static void EmitCommands(SourceWriter w, EntityModel entity)
    {
        string name = entity.Name;

        // Create command
        w.AppendLine($"public sealed record Create{name}Command : IRequest<Guid>");
        w.OpenBrace();
        EmitRequiredProperties(w, entity);
        w.CloseBrace();
        w.AppendLine();

        // Update command
        w.AppendLine($"public sealed record Update{name}Command : IRequest<bool>");
        w.OpenBrace();
        w.AppendLine("public Guid Id { get; init; }");
        EmitRequiredProperties(w, entity);
        w.CloseBrace();
        w.AppendLine();

        // Delete command
        w.AppendLine($"public sealed record Delete{name}Command(Guid Id) : IRequest<bool>;");
        w.AppendLine();
    }

    private static void EmitQueries(SourceWriter w, EntityModel entity)
    {
        string name = entity.Name;

        w.AppendLine($"public sealed record Get{name}ByIdQuery(Guid Id) : IRequest<{name}?>;");
        w.AppendLine();

        w.AppendLine($"public sealed record Get{name}ListQuery : IRequest<IReadOnlyList<{name}>>");
        w.OpenBrace();
        w.AppendLine("public int Skip { get; init; }");
        w.AppendLine("public int Take { get; init; } = 20;");

        // Add filter properties to query
        var filters = entity.FilterProperties.AsArray();
        for (int i = 0; i < filters.Length; i++)
        {
            var prop = filters[i];
            string nullableType = QueryFilterEmitter.GetNullableFilterType(prop);
            w.AppendLine($"public {nullableType} {prop.Name} {{ get; init; }}");
        }

        w.CloseBrace();
        w.AppendLine();
    }

    private static void EmitCreateHandler(SourceWriter w, EntityModel entity)
    {
        string name = entity.Name;

        w.AppendLine($"public sealed partial class Create{name}Handler : IRequestHandler<Create{name}Command, Guid>");
        w.OpenBrace();
        w.AppendLine("private readonly AppDbContext _db;");
        w.AppendLine();
        w.AppendLine($"public Create{name}Handler(AppDbContext db)");
        w.OpenBrace();
        w.AppendLine("_db = db;");
        w.CloseBrace();
        w.AppendLine();

        // Lifecycle hooks
        w.AppendLine($"partial void OnValidate(Create{name}Command command);");
        w.AppendLine($"partial void OnBeforeHandle(Create{name}Command command, {name} entity);");
        w.AppendLine($"partial void OnAfterHandle(Create{name}Command command, {name} entity);");
        w.AppendLine();

        // Handle method
        w.AppendLine($"public async Task<Guid> Handle(Create{name}Command command, CancellationToken cancellationToken)");
        w.OpenBrace();
        w.AppendLine("OnValidate(command);");
        w.AppendLine();
        w.AppendLine($"var entity = new {name}");
        w.OpenBrace();
        w.AppendLine("Id = Guid.NewGuid(),");

        var props = entity.Properties.AsArray();
        for (int i = 0; i < props.Length; i++)
        {
            if (!props[i].IsCollection)
            {
                w.AppendLine($"{props[i].Name} = command.{props[i].Name},");
            }
        }

        w.CloseBrace(semicolon: true);
        w.AppendLine();
        w.AppendLine("OnBeforeHandle(command, entity);");
        w.AppendLine();
        w.AppendLine("await RynorArchCrudRuntime.AddAndSaveAsync(_db, entity, cancellationToken);");
        w.AppendLine();
        w.AppendLine("OnAfterHandle(command, entity);");
        w.AppendLine();
        w.AppendLine("return entity.Id;");
        w.CloseBrace();
        w.CloseBrace();
        w.AppendLine();
    }

    private static void EmitUpdateHandler(SourceWriter w, EntityModel entity)
    {
        string name = entity.Name;

        w.AppendLine($"public sealed partial class Update{name}Handler : IRequestHandler<Update{name}Command, bool>");
        w.OpenBrace();
        w.AppendLine("private readonly AppDbContext _db;");
        w.AppendLine();
        w.AppendLine($"public Update{name}Handler(AppDbContext db)");
        w.OpenBrace();
        w.AppendLine("_db = db;");
        w.CloseBrace();
        w.AppendLine();

        w.AppendLine($"partial void OnValidate(Update{name}Command command);");
        w.AppendLine($"partial void OnBeforeHandle(Update{name}Command command, {name} entity);");
        w.AppendLine($"partial void OnAfterHandle(Update{name}Command command, {name} entity);");
        w.AppendLine();

        w.AppendLine($"public async Task<bool> Handle(Update{name}Command command, CancellationToken cancellationToken)");
        w.OpenBrace();
        w.AppendLine("OnValidate(command);");
        w.AppendLine();
        w.AppendLine($"var entity = await RynorArchCrudRuntime.FindByIdAsync<{name}>(_db, command.Id, cancellationToken);");
        w.AppendLine("if (entity is null) return false;");
        w.AppendLine();

        var props = entity.Properties.AsArray();
        for (int i = 0; i < props.Length; i++)
        {
            if (!props[i].IsCollection)
            {
                w.AppendLine($"entity.{props[i].Name} = command.{props[i].Name};");
            }
        }

        w.AppendLine();
        w.AppendLine("OnBeforeHandle(command, entity);");
        w.AppendLine();
        w.AppendLine("await RynorArchCrudRuntime.SaveUpdatedEntityAsync(_db, entity, cancellationToken);");
        w.AppendLine();
        w.AppendLine("OnAfterHandle(command, entity);");
        w.AppendLine();
        w.AppendLine("return true;");
        w.CloseBrace();
        w.CloseBrace();
        w.AppendLine();
    }

    private static void EmitDeleteHandler(SourceWriter w, EntityModel entity)
    {
        string name = entity.Name;

        w.AppendLine($"public sealed partial class Delete{name}Handler : IRequestHandler<Delete{name}Command, bool>");
        w.OpenBrace();
        w.AppendLine("private readonly AppDbContext _db;");
        w.AppendLine();
        w.AppendLine($"public Delete{name}Handler(AppDbContext db)");
        w.OpenBrace();
        w.AppendLine("_db = db;");
        w.CloseBrace();
        w.AppendLine();

        w.AppendLine($"partial void OnBeforeHandle(Delete{name}Command command, {name}? entity);");
        w.AppendLine($"partial void OnAfterHandle(Delete{name}Command command);");
        w.AppendLine();

        w.AppendLine($"public async Task<bool> Handle(Delete{name}Command command, CancellationToken cancellationToken)");
        w.OpenBrace();
        w.AppendLine($"var entity = await RynorArchCrudRuntime.FindByIdAsync<{name}>(_db, command.Id, cancellationToken);");
        w.AppendLine("if (entity is null) return false;");
        w.AppendLine();
        w.AppendLine("OnBeforeHandle(command, entity);");
        w.AppendLine();
        w.AppendLine("await RynorArchCrudRuntime.DeleteAndSaveAsync(_db, entity, cancellationToken);");
        w.AppendLine();
        w.AppendLine("OnAfterHandle(command);");
        w.AppendLine();
        w.AppendLine("return true;");
        w.CloseBrace();
        w.CloseBrace();
        w.AppendLine();
    }

    private static void EmitGetByIdHandler(SourceWriter w, EntityModel entity)
    {
        string name = entity.Name;

        w.AppendLine($"public sealed partial class Get{name}ByIdHandler : IRequestHandler<Get{name}ByIdQuery, {name}?>");
        w.OpenBrace();
        w.AppendLine("private readonly AppDbContext _db;");
        w.AppendLine();
        w.AppendLine($"public Get{name}ByIdHandler(AppDbContext db)");
        w.OpenBrace();
        w.AppendLine("_db = db;");
        w.CloseBrace();
        w.AppendLine();

        w.AppendLine($"partial void OnAfterHandle({name}? entity);");
        w.AppendLine();

        w.AppendLine($"public async Task<{name}?> Handle(Get{name}ByIdQuery query, CancellationToken cancellationToken)");
        w.OpenBrace();
        w.AppendLine($"var entity = await RynorArchCrudRuntime.FindByIdAsync<{name}>(_db, query.Id, cancellationToken);");
        w.AppendLine("OnAfterHandle(entity);");
        w.AppendLine("return entity;");
        w.CloseBrace();
        w.CloseBrace();
        w.AppendLine();
    }

    private static void EmitGetListHandler(SourceWriter w, EntityModel entity)
    {
        string name = entity.Name;

        w.AppendLine($"public sealed partial class Get{name}ListHandler : IRequestHandler<Get{name}ListQuery, IReadOnlyList<{name}>>");
        w.OpenBrace();
        w.AppendLine("private readonly AppDbContext _db;");
        w.AppendLine();
        w.AppendLine($"public Get{name}ListHandler(AppDbContext db)");
        w.OpenBrace();
        w.AppendLine("_db = db;");
        w.CloseBrace();
        w.AppendLine();

        w.AppendLine($"partial void OnBeforeQuery(Get{name}ListQuery query, ref IQueryable<{name}> queryable);");
        w.AppendLine();

        w.AppendLine($"public async Task<IReadOnlyList<{name}>> Handle(Get{name}ListQuery query, CancellationToken cancellationToken)");
        w.OpenBrace();
        w.AppendLine($"IQueryable<{name}> queryable = RynorArchCrudRuntime.CreateQuery<{name}>(_db);");
        w.AppendLine();

        // Apply filters from QueryFilter properties
        var filters = entity.FilterProperties.AsArray();
        for (int i = 0; i < filters.Length; i++)
        {
            QueryFilterEmitter.EmitQueryableFilter(w, filters[i], $"query.{filters[i].Name}");
        }

        w.AppendLine("OnBeforeQuery(query, ref queryable);");
        w.AppendLine();
        w.AppendLine("return await RynorArchCrudRuntime.ListAsync(queryable, query.Skip, query.Take, cancellationToken);");
        w.CloseBrace();
        w.CloseBrace();
    }

    private static void EmitRequiredProperties(SourceWriter w, EntityModel entity)
    {
        var props = entity.Properties.AsArray();
        for (int i = 0; i < props.Length; i++)
        {
            var prop = props[i];
            if (prop.IsCollection) continue;

            if (prop.IsNullable)
            {
                w.AppendLine($"public {prop.TypeName}? {prop.Name} {{ get; init; }}");
            }
            else
            {
                w.AppendLine($"public {prop.TypeName} {prop.Name} {{ get; init; }} = default!;");
            }
        }
    }
}
