using Microsoft.CodeAnalysis;
using ZenithArch.Generator.Helpers;
using ZenithArch.Generator.Models;

namespace ZenithArch.Generator.Emitters;

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
        w.AppendFileHeader("CQRS", entity.Name, "uses configured DbContext type for handlers");

        w.AppendLine("using System;");
        w.AppendLine("using System.Collections.Generic;");
        w.AppendLine("using System.Linq;");
        w.AppendLine("using MediatR;");
        w.AppendLine("using Microsoft.EntityFrameworkCore;");
        w.AppendLine("using ZenithArch.Abstractions.Interfaces;");
        w.AppendLine("using ZenithArch.Generated.Infrastructure;");
        w.AppendLine("using System.Threading;");
        w.AppendLine("using System.Threading.Tasks;");
        if (!string.IsNullOrEmpty(entity.Namespace))
        {
            w.AppendLine($"using {entity.Namespace};");
            if (entity.IsAggregateRoot)
            {
                w.AppendLine($"using {entity.Namespace}.DomainEvents;");
            }
        }
        w.AppendLine();

        if (!string.IsNullOrEmpty(entity.Namespace))
        {
            w.AppendLine($"namespace {entity.Namespace}.Cqrs;");
            w.AppendLine();
        }

        EmitCommands(w, entity, config);
        EmitQueries(w, entity);
        EmitCreateHandler(w, entity, config);
        EmitUpdateHandler(w, entity, config);
        EmitDeleteHandler(w, entity, config);
        EmitGetByIdHandler(w, entity, config);
        EmitGetListHandler(w, entity, config);

        return w.ToString();
    }

    private static void EmitCommands(SourceWriter w, EntityModel entity, ArchitectureConfig config)
    {
        string name = entity.Name;
        string writeCommandInterface = config.IsPerRequestTransactionSaveMode
            ? ", IZenithArchWriteCommand"
            : string.Empty;

        // Create command
        w.AppendLine($"public sealed record Create{name}Command : IRequest<Guid>{writeCommandInterface}");
        w.OpenBrace();
        EmitRequiredProperties(w, entity);
        w.CloseBrace();
        w.AppendLine();

        // Update command
        w.AppendLine($"public sealed record Update{name}Command : IRequest<bool>{writeCommandInterface}");
        w.OpenBrace();
        w.AppendLine("public Guid Id { get; init; }");
        EmitRequiredProperties(w, entity);
        w.CloseBrace();
        w.AppendLine();

        // Delete command
        w.AppendLine($"public sealed record Delete{name}Command(Guid Id) : IRequest<bool>{writeCommandInterface};");
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

    private static void EmitCreateHandler(SourceWriter w, EntityModel entity, ArchitectureConfig config)
    {
        string name = entity.Name;
        string dbContextType = config.CqrsDbContextTypeName;
        bool usesCacheInvalidation = config.GenerateCachingDecorators;
        bool usesPerRequestSave = config.IsPerRequestTransactionSaveMode;

        w.AppendLine($"public sealed partial class Create{name}Handler : IRequestHandler<Create{name}Command, Guid>");
        w.OpenBrace();
        w.AppendLine($"private readonly {dbContextType} _db;");
        w.AppendLine("private readonly ISecurityContext? _securityContext;");
        w.AppendLine("private readonly IEnumerable<IZenithArchExecutionObserver> _executionObservers;");
        if (usesCacheInvalidation)
        {
            w.AppendLine($"private readonly IEnumerable<IGet{name}ByIdCacheInvalidator> _cacheInvalidators;");
        }
        w.AppendLine();
        if (usesCacheInvalidation)
        {
            w.AppendLine($"public Create{name}Handler({dbContextType} db, IEnumerable<IGet{name}ByIdCacheInvalidator> cacheInvalidators, IEnumerable<ISecurityContext> securityContexts, IEnumerable<IZenithArchExecutionObserver> executionObservers)");
        }
        else
        {
            w.AppendLine($"public Create{name}Handler({dbContextType} db, IEnumerable<ISecurityContext> securityContexts, IEnumerable<IZenithArchExecutionObserver> executionObservers)");
        }
        w.OpenBrace();
        w.AppendLine("_db = db;");
        w.AppendLine("_securityContext = securityContexts.FirstOrDefault();");
        w.AppendLine("_executionObservers = executionObservers;");
        if (usesCacheInvalidation)
        {
            w.AppendLine("_cacheInvalidators = cacheInvalidators;");
        }
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
        w.AppendLine($"NotifyExecuting(\"Create{name}\", null);");
        w.AppendLine();
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
        w.AppendLine("if (entity is IAuditable auditable && !string.IsNullOrWhiteSpace(_securityContext?.UserId))");
        w.OpenBrace();
        w.AppendLine("auditable.CreatedBy ??= _securityContext!.UserId;");
        w.AppendLine("auditable.LastModifiedBy ??= _securityContext!.UserId;");
        w.CloseBrace();
        w.AppendLine();
        w.AppendLine("OnBeforeHandle(command, entity);");
        w.AppendLine();
        if (entity.IsAggregateRoot)
        {
            w.AppendLine($"entity.RaiseDomainEvent(new {name}CreatedEvent(entity.Id));");
            w.AppendLine();
        }
        if (usesPerRequestSave)
        {
            w.AppendLine("await ZenithArchCrudRuntime.AddAsync(_db, entity, cancellationToken);");
        }
        else
        {
            w.AppendLine("await ZenithArchCrudRuntime.AddAndSaveAsync(_db, entity, cancellationToken);");
        }
        if (usesCacheInvalidation)
        {
            w.AppendLine("await InvalidateCacheAsync(entity.Id, cancellationToken);");
        }
        w.AppendLine();
        w.AppendLine("OnAfterHandle(command, entity);");
        w.AppendLine($"NotifyCompleted(\"Create{name}\", entity.Id, success: true);");
        w.AppendLine();
        w.AppendLine("return entity.Id;");
        w.CloseBrace();

        EmitObserverMethods(w, name);

        if (usesCacheInvalidation)
        {
            EmitCacheInvalidationMethod(w);
        }

        w.CloseBrace();
        w.AppendLine();
    }

    private static void EmitUpdateHandler(SourceWriter w, EntityModel entity, ArchitectureConfig config)
    {
        string name = entity.Name;
        string dbContextType = config.CqrsDbContextTypeName;
        bool usesCacheInvalidation = config.GenerateCachingDecorators;
        bool usesPerRequestSave = config.IsPerRequestTransactionSaveMode;

        w.AppendLine($"public sealed partial class Update{name}Handler : IRequestHandler<Update{name}Command, bool>");
        w.OpenBrace();
        w.AppendLine($"private readonly {dbContextType} _db;");
        w.AppendLine("private readonly ISecurityContext? _securityContext;");
        w.AppendLine("private readonly IEnumerable<IZenithArchExecutionObserver> _executionObservers;");
        if (usesCacheInvalidation)
        {
            w.AppendLine($"private readonly IEnumerable<IGet{name}ByIdCacheInvalidator> _cacheInvalidators;");
        }
        w.AppendLine();
        if (usesCacheInvalidation)
        {
            w.AppendLine($"public Update{name}Handler({dbContextType} db, IEnumerable<IGet{name}ByIdCacheInvalidator> cacheInvalidators, IEnumerable<ISecurityContext> securityContexts, IEnumerable<IZenithArchExecutionObserver> executionObservers)");
        }
        else
        {
            w.AppendLine($"public Update{name}Handler({dbContextType} db, IEnumerable<ISecurityContext> securityContexts, IEnumerable<IZenithArchExecutionObserver> executionObservers)");
        }
        w.OpenBrace();
        w.AppendLine("_db = db;");
        w.AppendLine("_securityContext = securityContexts.FirstOrDefault();");
        w.AppendLine("_executionObservers = executionObservers;");
        if (usesCacheInvalidation)
        {
            w.AppendLine("_cacheInvalidators = cacheInvalidators;");
        }
        w.CloseBrace();
        w.AppendLine();

        w.AppendLine($"partial void OnValidate(Update{name}Command command);");
        w.AppendLine($"partial void OnBeforeHandle(Update{name}Command command, {name} entity);");
        w.AppendLine($"partial void OnAfterHandle(Update{name}Command command, {name} entity);");
        w.AppendLine();

        w.AppendLine($"public async Task<bool> Handle(Update{name}Command command, CancellationToken cancellationToken)");
        w.OpenBrace();
        w.AppendLine($"NotifyExecuting(\"Update{name}\", command.Id);");
        w.AppendLine();
        w.AppendLine("OnValidate(command);");
        w.AppendLine();
        w.AppendLine($"var entity = await ZenithArchCrudRuntime.FindByIdAsync<{name}>(_db, command.Id, cancellationToken);");
        w.AppendLine("if (entity is null)");
        w.OpenBrace();
        w.AppendLine($"NotifyCompleted(\"Update{name}\", command.Id, success: false);");
        w.AppendLine("return false;");
        w.CloseBrace();
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
        w.AppendLine("if (entity is IAuditable auditable && !string.IsNullOrWhiteSpace(_securityContext?.UserId))");
        w.OpenBrace();
        w.AppendLine("auditable.LastModifiedBy = _securityContext!.UserId;");
        w.CloseBrace();
        w.AppendLine();
        w.AppendLine("OnBeforeHandle(command, entity);");
        w.AppendLine();
        if (entity.IsAggregateRoot)
        {
            w.AppendLine($"entity.RaiseDomainEvent(new {name}UpdatedEvent(entity.Id));");
            w.AppendLine();
        }
        if (usesPerRequestSave)
        {
            w.AppendLine("ZenithArchCrudRuntime.MarkUpdated(_db, entity);");
        }
        else
        {
            w.AppendLine("await ZenithArchCrudRuntime.SaveUpdatedEntityAsync(_db, entity, cancellationToken);");
        }
        if (usesCacheInvalidation)
        {
            w.AppendLine("await InvalidateCacheAsync(entity.Id, cancellationToken);");
        }
        w.AppendLine();
        w.AppendLine("OnAfterHandle(command, entity);");
        w.AppendLine($"NotifyCompleted(\"Update{name}\", entity.Id, success: true);");
        w.AppendLine();
        w.AppendLine("return true;");
        w.CloseBrace();

        EmitObserverMethods(w, name);

        if (usesCacheInvalidation)
        {
            EmitCacheInvalidationMethod(w);
        }

        w.CloseBrace();
        w.AppendLine();
    }

    private static void EmitDeleteHandler(SourceWriter w, EntityModel entity, ArchitectureConfig config)
    {
        string name = entity.Name;
        string dbContextType = config.CqrsDbContextTypeName;
        bool usesCacheInvalidation = config.GenerateCachingDecorators;
        bool usesPerRequestSave = config.IsPerRequestTransactionSaveMode;

        w.AppendLine($"public sealed partial class Delete{name}Handler : IRequestHandler<Delete{name}Command, bool>");
        w.OpenBrace();
        w.AppendLine($"private readonly {dbContextType} _db;");
        w.AppendLine("private readonly ISecurityContext? _securityContext;");
        w.AppendLine("private readonly IEnumerable<IZenithArchExecutionObserver> _executionObservers;");
        if (usesCacheInvalidation)
        {
            w.AppendLine($"private readonly IEnumerable<IGet{name}ByIdCacheInvalidator> _cacheInvalidators;");
        }
        w.AppendLine();
        if (usesCacheInvalidation)
        {
            w.AppendLine($"public Delete{name}Handler({dbContextType} db, IEnumerable<IGet{name}ByIdCacheInvalidator> cacheInvalidators, IEnumerable<ISecurityContext> securityContexts, IEnumerable<IZenithArchExecutionObserver> executionObservers)");
        }
        else
        {
            w.AppendLine($"public Delete{name}Handler({dbContextType} db, IEnumerable<ISecurityContext> securityContexts, IEnumerable<IZenithArchExecutionObserver> executionObservers)");
        }
        w.OpenBrace();
        w.AppendLine("_db = db;");
        w.AppendLine("_securityContext = securityContexts.FirstOrDefault();");
        w.AppendLine("_executionObservers = executionObservers;");
        if (usesCacheInvalidation)
        {
            w.AppendLine("_cacheInvalidators = cacheInvalidators;");
        }
        w.CloseBrace();
        w.AppendLine();

        w.AppendLine($"partial void OnBeforeHandle(Delete{name}Command command, {name}? entity);");
        w.AppendLine($"partial void OnAfterHandle(Delete{name}Command command);");
        w.AppendLine();

        w.AppendLine($"public async Task<bool> Handle(Delete{name}Command command, CancellationToken cancellationToken)");
        w.OpenBrace();
        w.AppendLine($"NotifyExecuting(\"Delete{name}\", command.Id);");
        w.AppendLine();
        w.AppendLine($"var entity = await ZenithArchCrudRuntime.FindByIdAsync<{name}>(_db, command.Id, cancellationToken);");
        w.AppendLine("if (entity is null)");
        w.OpenBrace();
        w.AppendLine($"NotifyCompleted(\"Delete{name}\", command.Id, success: false);");
        w.AppendLine("return false;");
        w.CloseBrace();
        w.AppendLine();
        w.AppendLine("if (entity is IAuditable auditable && !string.IsNullOrWhiteSpace(_securityContext?.UserId))");
        w.OpenBrace();
        w.AppendLine("auditable.LastModifiedBy = _securityContext!.UserId;");
        w.CloseBrace();
        w.AppendLine();
        w.AppendLine("OnBeforeHandle(command, entity);");
        w.AppendLine();
        if (entity.IsAggregateRoot)
        {
            w.AppendLine($"entity.RaiseDomainEvent(new {name}DeletedEvent(entity.Id));");
            w.AppendLine();
        }
        if (usesPerRequestSave)
        {
            w.AppendLine("ZenithArchCrudRuntime.Delete(_db, entity);");
        }
        else
        {
            w.AppendLine("await ZenithArchCrudRuntime.DeleteAndSaveAsync(_db, entity, cancellationToken);");
        }
        if (usesCacheInvalidation)
        {
            w.AppendLine("await InvalidateCacheAsync(command.Id, cancellationToken);");
        }
        w.AppendLine();
        w.AppendLine("OnAfterHandle(command);");
        w.AppendLine($"NotifyCompleted(\"Delete{name}\", command.Id, success: true);");
        w.AppendLine();
        w.AppendLine("return true;");
        w.CloseBrace();

        EmitObserverMethods(w, name);

        if (usesCacheInvalidation)
        {
            EmitCacheInvalidationMethod(w);
        }

        w.CloseBrace();
        w.AppendLine();
    }

    private static void EmitGetByIdHandler(SourceWriter w, EntityModel entity, ArchitectureConfig config)
    {
        string name = entity.Name;
        string dbContextType = config.CqrsDbContextTypeName;

        w.AppendLine($"public sealed partial class Get{name}ByIdHandler : IRequestHandler<Get{name}ByIdQuery, {name}?>");
        w.OpenBrace();
        w.AppendLine($"private readonly {dbContextType} _db;");
        w.AppendLine("private readonly ISecurityContext? _securityContext;");
        w.AppendLine("private readonly IEnumerable<IZenithArchExecutionObserver> _executionObservers;");
        w.AppendLine();
        w.AppendLine($"public Get{name}ByIdHandler({dbContextType} db, IEnumerable<ISecurityContext> securityContexts, IEnumerable<IZenithArchExecutionObserver> executionObservers)");
        w.OpenBrace();
        w.AppendLine("_db = db;");
        w.AppendLine("_securityContext = securityContexts.FirstOrDefault();");
        w.AppendLine("_executionObservers = executionObservers;");
        w.CloseBrace();
        w.AppendLine();

        w.AppendLine($"partial void OnAfterHandle({name}? entity);");
        w.AppendLine();

        w.AppendLine($"public async Task<{name}?> Handle(Get{name}ByIdQuery query, CancellationToken cancellationToken)");
        w.OpenBrace();
        w.AppendLine($"NotifyExecuting(\"Get{name}ById\", query.Id);");
        w.AppendLine($"var entity = await ZenithArchCrudRuntime.FindByIdAsync<{name}>(_db, query.Id, cancellationToken);");
        w.AppendLine("OnAfterHandle(entity);");
        w.AppendLine($"NotifyCompleted(\"Get{name}ById\", query.Id, entity is not null);");
        w.AppendLine("return entity;");
        w.CloseBrace();

        EmitObserverMethods(w, name);
        w.CloseBrace();
        w.AppendLine();
    }

    private static void EmitGetListHandler(SourceWriter w, EntityModel entity, ArchitectureConfig config)
    {
        string name = entity.Name;
        string dbContextType = config.CqrsDbContextTypeName;

        w.AppendLine($"public sealed partial class Get{name}ListHandler : IRequestHandler<Get{name}ListQuery, IReadOnlyList<{name}>>");
        w.OpenBrace();
        w.AppendLine($"private readonly {dbContextType} _db;");
        w.AppendLine("private readonly ISecurityContext? _securityContext;");
        w.AppendLine("private readonly IEnumerable<IZenithArchExecutionObserver> _executionObservers;");
        w.AppendLine();
        w.AppendLine($"public Get{name}ListHandler({dbContextType} db, IEnumerable<ISecurityContext> securityContexts, IEnumerable<IZenithArchExecutionObserver> executionObservers)");
        w.OpenBrace();
        w.AppendLine("_db = db;");
        w.AppendLine("_securityContext = securityContexts.FirstOrDefault();");
        w.AppendLine("_executionObservers = executionObservers;");
        w.CloseBrace();
        w.AppendLine();

        w.AppendLine($"partial void OnBeforeQuery(Get{name}ListQuery query, ref IQueryable<{name}> queryable);");
        w.AppendLine();

        w.AppendLine($"public async Task<IReadOnlyList<{name}>> Handle(Get{name}ListQuery query, CancellationToken cancellationToken)");
        w.OpenBrace();
        w.AppendLine($"NotifyExecuting(\"Get{name}List\", null);");
        w.AppendLine($"IQueryable<{name}> queryable = ZenithArchCrudRuntime.CreateQuery<{name}>(_db);");
        w.AppendLine();

        // Apply filters from QueryFilter properties
        var filters = entity.FilterProperties.AsArray();
        for (int i = 0; i < filters.Length; i++)
        {
            QueryFilterEmitter.EmitQueryableFilter(w, filters[i], $"query.{filters[i].Name}");
        }

        w.AppendLine("OnBeforeQuery(query, ref queryable);");
        w.AppendLine();
        w.AppendLine("var results = await ZenithArchCrudRuntime.ListAsync(queryable, query.Skip, query.Take, cancellationToken);");
        w.AppendLine($"NotifyCompleted(\"Get{name}List\", null, success: true);");
        w.AppendLine("return results;");
        w.CloseBrace();

        EmitObserverMethods(w, name);
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

    private static void EmitCacheInvalidationMethod(SourceWriter w)
    {
        w.AppendLine();
        w.AppendLine("private async Task InvalidateCacheAsync(Guid id, CancellationToken cancellationToken)");
        w.OpenBrace();
        w.AppendLine("foreach (var invalidator in _cacheInvalidators)");
        w.OpenBrace();
        w.AppendLine("await invalidator.InvalidateAsync(id, cancellationToken);");
        w.CloseBrace();
        w.CloseBrace();
    }

    private static void EmitObserverMethods(SourceWriter w, string entityName)
    {
        w.AppendLine();
        w.AppendLine("private void NotifyExecuting(string operation, Guid? entityId)");
        w.OpenBrace();
        w.AppendLine("foreach (var observer in _executionObservers)");
        w.OpenBrace();
        w.AppendLine($"observer.OnHandlerExecuting(operation, \"{entityName}\", entityId, _securityContext?.UserId, _securityContext?.TenantId);");
        w.CloseBrace();
        w.CloseBrace();
        w.AppendLine();
        w.AppendLine("private void NotifyCompleted(string operation, Guid? entityId, bool success)");
        w.OpenBrace();
        w.AppendLine("foreach (var observer in _executionObservers)");
        w.OpenBrace();
        w.AppendLine($"observer.OnHandlerCompleted(operation, \"{entityName}\", entityId, success);");
        w.CloseBrace();
        w.CloseBrace();
    }
}
