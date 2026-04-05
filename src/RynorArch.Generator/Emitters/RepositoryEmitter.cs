using Microsoft.CodeAnalysis;
using RynorArch.Generator.Helpers;
using RynorArch.Generator.Models;

namespace RynorArch.Generator.Emitters;

/// <summary>
/// Generates Repository pattern artifacts: IRepository interface, concrete Repository, and optional IUnitOfWork.
/// </summary>
internal static class RepositoryEmitter
{
    public static void Emit(SourceProductionContext context, EntityModel entity, ArchitectureConfig config)
    {
        var source = Generate(entity, config);
        context.AddSource($"{entity.Name}.Repository.g.cs", source);

        // Emit UnitOfWork once per compilation (tracked externally by caller)
        if (config.UseUnitOfWork)
        {
            var uowSource = GenerateUnitOfWorkInterface();
            context.AddSource("IUnitOfWork.g.cs", uowSource);
        }
    }

    internal static string Generate(EntityModel entity, ArchitectureConfig config)
    {
        var w = new SourceWriter(2048);
        w.AppendFileHeader();

        w.AppendLine("using System.Linq.Expressions;");
        w.AppendLine("using Microsoft.EntityFrameworkCore;");

        if (config.UseSpecification)
        {
            w.AppendLine("using RynorArch.Abstractions.Interfaces;");
        }

        w.AppendLine();

        if (!string.IsNullOrEmpty(entity.Namespace))
        {
            w.AppendLine($"namespace {entity.Namespace}.Repositories;");
            w.AppendLine();
        }

        EmitInterface(w, entity, config);
        w.AppendLine();
        EmitImplementation(w, entity, config);

        return w.ToString();
    }

    private static void EmitInterface(SourceWriter w, EntityModel entity, ArchitectureConfig config)
    {
        string name = entity.Name;

        w.AppendLine($"public interface I{name}Repository");
        w.OpenBrace();
        w.AppendLine($"Task<{name}?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);");
        w.AppendLine($"Task<IReadOnlyList<{name}>> GetAllAsync(CancellationToken cancellationToken = default);");
        w.AppendLine($"Task<{name}> AddAsync({name} entity, CancellationToken cancellationToken = default);");
        w.AppendLine($"Task UpdateAsync({name} entity, CancellationToken cancellationToken = default);");
        w.AppendLine($"Task DeleteAsync({name} entity, CancellationToken cancellationToken = default);");
        w.AppendLine($"Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);");

        if (config.UseSpecification)
        {
            w.AppendLine($"Task<IReadOnlyList<{name}>> ListAsync(ISpecification<{name}> specification, CancellationToken cancellationToken = default);");
            w.AppendLine($"Task<int> CountAsync(ISpecification<{name}> specification, CancellationToken cancellationToken = default);");
        }

        w.CloseBrace();
    }

    private static void EmitImplementation(SourceWriter w, EntityModel entity, ArchitectureConfig config)
    {
        string name = entity.Name;

        w.AppendLine($"public sealed partial class {name}Repository : I{name}Repository");
        w.OpenBrace();
        w.AppendLine("private readonly DbContext _db;");
        w.AppendLine();
        w.AppendLine($"public {name}Repository(DbContext db)");
        w.OpenBrace();
        w.AppendLine("_db = db;");
        w.CloseBrace();
        w.AppendLine();

        // GetById
        w.AppendLine($"public async Task<{name}?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)");
        w.OpenBrace();
        w.AppendLine($"return await _db.Set<{name}>().FindAsync(new object[] {{ id }}, cancellationToken);");
        w.CloseBrace();
        w.AppendLine();

        // GetAll
        w.AppendLine($"public async Task<IReadOnlyList<{name}>> GetAllAsync(CancellationToken cancellationToken = default)");
        w.OpenBrace();
        w.AppendLine($"return await _db.Set<{name}>().AsNoTracking().ToListAsync(cancellationToken);");
        w.CloseBrace();
        w.AppendLine();

        // Add
        w.AppendLine($"public async Task<{name}> AddAsync({name} entity, CancellationToken cancellationToken = default)");
        w.OpenBrace();
        w.AppendLine($"await _db.Set<{name}>().AddAsync(entity, cancellationToken);");
        w.AppendLine("await _db.SaveChangesAsync(cancellationToken);");
        w.AppendLine("return entity;");
        w.CloseBrace();
        w.AppendLine();

        // Update
        w.AppendLine($"public Task UpdateAsync({name} entity, CancellationToken cancellationToken = default)");
        w.OpenBrace();
        w.AppendLine($"_db.Set<{name}>().Update(entity);");
        w.AppendLine("return _db.SaveChangesAsync(cancellationToken);");
        w.CloseBrace();
        w.AppendLine();

        // Delete
        w.AppendLine($"public Task DeleteAsync({name} entity, CancellationToken cancellationToken = default)");
        w.OpenBrace();
        w.AppendLine($"_db.Set<{name}>().Remove(entity);");
        w.AppendLine("return _db.SaveChangesAsync(cancellationToken);");
        w.CloseBrace();
        w.AppendLine();

        // Exists
        w.AppendLine($"public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)");
        w.OpenBrace();
        w.AppendLine($"return await _db.Set<{name}>().AnyAsync(e => e.Id == id, cancellationToken);");
        w.CloseBrace();

        if (config.UseSpecification)
        {
            w.AppendLine();
            EmitSpecificationMethods(w, entity);
        }

        w.CloseBrace();
    }

    private static void EmitSpecificationMethods(SourceWriter w, EntityModel entity)
    {
        string name = entity.Name;

        // List with specification
        w.AppendLine($"public async Task<IReadOnlyList<{name}>> ListAsync(ISpecification<{name}> specification, CancellationToken cancellationToken = default)");
        w.OpenBrace();
        w.AppendLine($"IQueryable<{name}> queryable = _db.Set<{name}>().AsNoTracking();");
        w.AppendLine("queryable = ApplySpecification(queryable, specification);");
        w.AppendLine("return await queryable.ToListAsync(cancellationToken);");
        w.CloseBrace();
        w.AppendLine();

        // Count with specification
        w.AppendLine($"public async Task<int> CountAsync(ISpecification<{name}> specification, CancellationToken cancellationToken = default)");
        w.OpenBrace();
        w.AppendLine($"IQueryable<{name}> queryable = _db.Set<{name}>().AsNoTracking();");
        w.AppendLine("if (specification.Criteria is not null)");
        w.IncreaseIndent();
        w.AppendLine("queryable = queryable.Where(specification.Criteria);");
        w.DecreaseIndent();
        w.AppendLine("return await queryable.CountAsync(cancellationToken);");
        w.CloseBrace();
        w.AppendLine();

        // ApplySpecification helper
        w.AppendLine($"private static IQueryable<{name}> ApplySpecification(IQueryable<{name}> queryable, ISpecification<{name}> specification)");
        w.OpenBrace();
        w.AppendLine("if (specification.Criteria is not null)");
        w.IncreaseIndent();
        w.AppendLine("queryable = queryable.Where(specification.Criteria);");
        w.DecreaseIndent();
        w.AppendLine();
        w.AppendLine("for (int i = 0; i < specification.Includes.Count; i++)");
        w.IncreaseIndent();
        w.AppendLine("queryable = queryable.Include(specification.Includes[i]);");
        w.DecreaseIndent();
        w.AppendLine();
        w.AppendLine("if (specification.Skip.HasValue)");
        w.IncreaseIndent();
        w.AppendLine("queryable = queryable.Skip(specification.Skip.Value);");
        w.DecreaseIndent();
        w.AppendLine();
        w.AppendLine("if (specification.Take.HasValue)");
        w.IncreaseIndent();
        w.AppendLine("queryable = queryable.Take(specification.Take.Value);");
        w.DecreaseIndent();
        w.AppendLine();
        w.AppendLine("return queryable;");
        w.CloseBrace();
    }

    internal static string GenerateUnitOfWorkInterface()
    {
        var w = new SourceWriter(512);
        w.AppendFileHeader();

        w.AppendLine("/// <summary>");
        w.AppendLine("/// Unit of Work interface for coordinating repository transactions.");
        w.AppendLine("/// </summary>");
        w.AppendLine("public interface IUnitOfWork : IDisposable");
        w.OpenBrace();
        w.AppendLine("Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);");
        w.CloseBrace();

        return w.ToString();
    }
}
