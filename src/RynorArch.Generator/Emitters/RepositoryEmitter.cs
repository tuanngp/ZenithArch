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
    }

    internal static string Generate(EntityModel entity, ArchitectureConfig config)
    {
        var w = new SourceWriter(1024);
        w.AppendFileHeader();

        w.AppendLine("using Microsoft.EntityFrameworkCore;");
        w.AppendLine("using RynorArch.Generated.Infrastructure;");
        if (!string.IsNullOrEmpty(entity.Namespace))
        {
            w.AppendLine($"using {entity.Namespace};");
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

        _ = config;
        w.AppendLine($"public interface I{name}Repository : ICrudRepository<{name}>;");
    }

    private static void EmitImplementation(SourceWriter w, EntityModel entity, ArchitectureConfig config)
    {
        string name = entity.Name;

        _ = config;
        w.AppendLine($"public sealed partial class {name}Repository : CrudRepository<{name}>, I{name}Repository");
        w.OpenBrace();
        w.AppendLine($"public {name}Repository(DbContext db) : base(db)");
        w.OpenBrace();
        w.CloseBrace();
        w.CloseBrace();
    }

    internal static string GenerateUnitOfWorkInterface()
    {
        var w = new SourceWriter(512);
        w.AppendFileHeader();

        w.AppendLine("using System;");
        w.AppendLine("using System.Threading;");
        w.AppendLine("using System.Threading.Tasks;");
        w.AppendLine();
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
