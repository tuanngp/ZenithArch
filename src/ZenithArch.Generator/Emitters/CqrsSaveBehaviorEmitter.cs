using Microsoft.CodeAnalysis;
using ZenithArch.Generator.Helpers;

namespace ZenithArch.Generator.Emitters;

internal static class CqrsSaveBehaviorEmitter
{
    public static void Emit(SourceProductionContext context, string dbContextTypeName)
    {
        var w = new SourceWriter(2048);
        w.AppendFileHeader("Global.CqrsSaveBehavior");

        w.AppendLine("using System;");
        w.AppendLine("using System.Threading;");
        w.AppendLine("using System.Threading.Tasks;");
        w.AppendLine("using MediatR;");
        w.AppendLine("using Microsoft.EntityFrameworkCore.Storage;");
        w.AppendLine();
        w.AppendLine("namespace ZenithArch.Generated.Infrastructure;");
        w.AppendLine();

        w.AppendLine("public interface IZenithArchWriteCommand;");
        w.AppendLine();

        w.AppendLine("public sealed class ZenithArchSaveChangesBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>");
        w.AppendLine("    where TRequest : IZenithArchWriteCommand");
        w.OpenBrace();
        w.AppendLine($"private readonly {dbContextTypeName} _db;");
        w.AppendLine();
        w.AppendLine($"public ZenithArchSaveChangesBehavior({dbContextTypeName} db)");
        w.OpenBrace();
        w.AppendLine("_db = db;");
        w.CloseBrace();
        w.AppendLine();
        w.AppendLine("public async Task<TResponse> Handle(");
        w.IncreaseIndent();
        w.AppendLine("TRequest request,");
        w.AppendLine("RequestHandlerDelegate<TResponse> next,");
        w.AppendLine("CancellationToken cancellationToken)");
        w.DecreaseIndent();
        w.OpenBrace();
        w.AppendLine("IDbContextTransaction? transaction = null;");
        w.AppendLine("var ownsTransaction = _db.Database.CurrentTransaction is null;");
        w.AppendLine("if (ownsTransaction)");
        w.OpenBrace();
        w.AppendLine("transaction = await _db.Database.BeginTransactionAsync(cancellationToken);");
        w.CloseBrace();
        w.AppendLine();
        w.AppendLine("try");
        w.OpenBrace();
        w.AppendLine("var response = await next();");
        w.AppendLine("await _db.SaveChangesAsync(cancellationToken);");
        w.AppendLine("if (transaction is not null)");
        w.OpenBrace();
        w.AppendLine("await transaction.CommitAsync(cancellationToken);");
        w.CloseBrace();
        w.AppendLine("return response;");
        w.CloseBrace();
        w.AppendLine("catch");
        w.OpenBrace();
        w.AppendLine("if (transaction is not null)");
        w.OpenBrace();
        w.AppendLine("await transaction.RollbackAsync(cancellationToken);");
        w.CloseBrace();
        w.AppendLine("throw;");
        w.CloseBrace();
        w.AppendLine("finally");
        w.OpenBrace();
        w.AppendLine("if (transaction is not null)");
        w.OpenBrace();
        w.AppendLine("await transaction.DisposeAsync();");
        w.CloseBrace();
        w.CloseBrace();
        w.CloseBrace();
        w.CloseBrace();

        context.AddSource("ZenithArch.CqrsSaveBehavior.g.cs", w.ToString());
    }
}
