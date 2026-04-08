using Microsoft.CodeAnalysis;
using RynorArch.Generator.Helpers;

namespace RynorArch.Generator.Emitters;

internal static class ValidationPipelineBehaviorEmitter
{
    public static void Emit(SourceProductionContext context)
    {
        var w = new SourceWriter(2048);
        w.AppendFileHeader("Global.ValidationBehavior");

        w.AppendLine("using System.Collections.Generic;");
        w.AppendLine("using System.Threading;");
        w.AppendLine("using System.Threading.Tasks;");
        w.AppendLine("using FluentValidation;");
        w.AppendLine("using FluentValidation.Results;");
        w.AppendLine("using MediatR;");
        w.AppendLine();
        w.AppendLine("namespace RynorArch.Generated.Infrastructure;");
        w.AppendLine();

        w.AppendLine("public sealed class RynorArchValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>");
        w.AppendLine("    where TRequest : notnull");
        w.OpenBrace();
        w.AppendLine("private readonly IEnumerable<IValidator<TRequest>> _validators;");
        w.AppendLine();
        w.AppendLine("public RynorArchValidationBehavior(IEnumerable<IValidator<TRequest>> validators)");
        w.OpenBrace();
        w.AppendLine("_validators = validators;");
        w.CloseBrace();
        w.AppendLine();
        w.AppendLine("public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)");
        w.OpenBrace();
        w.AppendLine("if (_validators is ICollection<IValidator<TRequest>> validatorCollection && validatorCollection.Count == 0)");
        w.OpenBrace();
        w.AppendLine("return await next();");
        w.CloseBrace();
        w.AppendLine();
        w.AppendLine("List<ValidationFailure>? failures = null;");
        w.AppendLine("var validationContext = new ValidationContext<TRequest>(request);");
        w.AppendLine();
        w.AppendLine("foreach (var validator in _validators)");
        w.OpenBrace();
        w.AppendLine("var result = await validator.ValidateAsync(validationContext, cancellationToken);");
        w.AppendLine("if (!result.IsValid)");
        w.OpenBrace();
        w.AppendLine("failures ??= new List<ValidationFailure>();");
        w.AppendLine("failures.AddRange(result.Errors);");
        w.CloseBrace();
        w.CloseBrace();
        w.AppendLine();
        w.AppendLine("if (failures is not null)");
        w.OpenBrace();
        w.AppendLine("throw new ValidationException(failures);");
        w.CloseBrace();
        w.AppendLine();
        w.AppendLine("return await next();");
        w.CloseBrace();
        w.CloseBrace();

        context.AddSource("RynorArch.ValidationBehavior.g.cs", w.ToString());
    }
}