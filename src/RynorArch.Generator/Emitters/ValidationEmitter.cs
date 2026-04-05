using Microsoft.CodeAnalysis;
using RynorArch.Generator.Helpers;
using RynorArch.Generator.Models;

namespace RynorArch.Generator.Emitters;

/// <summary>
/// Generates FluentValidation validator stubs for Create commands.
/// Produces empty validator classes that users can extend with rules.
/// </summary>
internal static class ValidationEmitter
{
    public static void Emit(SourceProductionContext context, EntityModel entity)
    {
        var source = Generate(entity);
        context.AddSource($"{entity.Name}.Validation.g.cs", source);
    }

    internal static string Generate(EntityModel entity)
    {
        var w = new SourceWriter(1024);
        w.AppendFileHeader();

        w.AppendLine("using FluentValidation;");
        w.AppendLine();

        string cqrsNamespace = string.IsNullOrEmpty(entity.Namespace)
            ? "Cqrs"
            : $"{entity.Namespace}.Cqrs";

        if (!string.IsNullOrEmpty(entity.Namespace))
        {
            w.AppendLine($"namespace {entity.Namespace}.Validation;");
            w.AppendLine();
        }

        string name = entity.Name;

        // Create validator
        w.AppendLine($"public partial class Create{name}Validator : AbstractValidator<{cqrsNamespace}.Create{name}Command>");
        w.OpenBrace();
        w.AppendLine($"public Create{name}Validator()");
        w.OpenBrace();

        // Generate basic rules for required string properties
        var props = entity.Properties.AsArray();
        for (int i = 0; i < props.Length; i++)
        {
            var prop = props[i];
            if (prop.IsCollection || prop.IsNullable) continue;

            if (prop.TypeName.IndexOf("string", System.StringComparison.Ordinal) >= 0)
            {
                w.AppendLine($"RuleFor(x => x.{prop.Name}).NotEmpty();");
            }
        }

        w.AppendLine("ConfigureRules();");
        w.CloseBrace();
        w.AppendLine();
        w.AppendLine("/// <summary>Extend this method in a partial class to add custom validation rules.</summary>");
        w.AppendLine("partial void ConfigureRules();");
        w.CloseBrace();
        w.AppendLine();

        // Update validator
        w.AppendLine($"public partial class Update{name}Validator : AbstractValidator<{cqrsNamespace}.Update{name}Command>");
        w.OpenBrace();
        w.AppendLine($"public Update{name}Validator()");
        w.OpenBrace();
        w.AppendLine("RuleFor(x => x.Id).NotEmpty();");

        for (int i = 0; i < props.Length; i++)
        {
            var prop = props[i];
            if (prop.IsCollection || prop.IsNullable) continue;

            if (prop.TypeName.IndexOf("string", System.StringComparison.Ordinal) >= 0)
            {
                w.AppendLine($"RuleFor(x => x.{prop.Name}).NotEmpty();");
            }
        }

        w.AppendLine("ConfigureRules();");
        w.CloseBrace();
        w.AppendLine();
        w.AppendLine("/// <summary>Extend this method in a partial class to add custom validation rules.</summary>");
        w.AppendLine("partial void ConfigureRules();");
        w.CloseBrace();

        return w.ToString();
    }
}
