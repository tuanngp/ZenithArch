using Microsoft.CodeAnalysis;
using ZenithArch.Generator.Helpers;

namespace ZenithArch.Generator.Emitters;

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
        w.AppendLine("using ZenithArch.Abstractions.Interfaces;");
        w.AppendLine();
        w.AppendLine("namespace ZenithArch.Generated.Infrastructure;");
        w.AppendLine();

        w.AppendLine("public sealed class ZenithArchValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>");
        w.AppendLine("    where TRequest : notnull");
        w.OpenBrace();
        w.AppendLine("private readonly IEnumerable<IValidator<TRequest>> _validators;");
        w.AppendLine("private readonly IEnumerable<IZenithArchExecutionObserver> _executionObservers;");
        w.AppendLine();
        w.AppendLine("public ZenithArchValidationBehavior(IEnumerable<IValidator<TRequest>> validators, IEnumerable<IZenithArchExecutionObserver> executionObservers)");
        w.OpenBrace();
        w.AppendLine("_validators = validators;");
        w.AppendLine("_executionObservers = executionObservers;");
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
        w.AppendLine("NotifyValidationFailure(typeof(TRequest).Name, failures.Count);");
        w.AppendLine("throw new ValidationException(failures);");
        w.CloseBrace();
        w.AppendLine();
        w.AppendLine("return await next();");
        w.CloseBrace();
        w.AppendLine();
        w.AppendLine("private void NotifyValidationFailure(string requestName, int failureCount)");
        w.OpenBrace();
        w.AppendLine("foreach (var observer in _executionObservers)");
        w.OpenBrace();
        w.AppendLine("observer.OnValidationFailed(requestName, failureCount);");
        w.CloseBrace();
        w.CloseBrace();
        w.CloseBrace();

        context.AddSource("ZenithArch.ValidationBehavior.g.cs", w.ToString());
    }
}