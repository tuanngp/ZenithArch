using Microsoft.CodeAnalysis;
using ZenithArch.Generator.Helpers;
using ZenithArch.Generator.Models;

namespace ZenithArch.Generator.Emitters;

/// <summary>
/// Generates Specification classes from [QueryFilter] properties.
/// Produces a concrete specification per entity that builds criteria from nullable filter inputs.
/// </summary>
internal static class SpecificationEmitter
{
    public static void Emit(SourceProductionContext context, EntityModel entity)
    {
        if (entity.FilterProperties.Length == 0) return;

        var source = Generate(entity);
        context.AddSource($"{entity.Name}.Specification.g.cs", source);
    }

    internal static string Generate(EntityModel entity)
    {
        var w = new SourceWriter(2048);
        w.AppendFileHeader("Specification", entity.Name);

        w.AppendLine("using System;");
        w.AppendLine("using System.Collections.Generic;");
        w.AppendLine("using System.Linq.Expressions;");
        w.AppendLine("using ZenithArch.Abstractions.Interfaces;");
        if (!string.IsNullOrEmpty(entity.Namespace))
        {
            w.AppendLine($"using {entity.Namespace};");
        }
        w.AppendLine();

        if (!string.IsNullOrEmpty(entity.Namespace))
        {
            w.AppendLine($"namespace {entity.Namespace}.Specifications;");
            w.AppendLine();
        }

        EmitSpecificationClass(w, entity);

        return w.ToString();
    }

    private static void EmitSpecificationClass(SourceWriter w, EntityModel entity)
    {
        string name = entity.Name;
        var filters = entity.FilterProperties.AsArray();

        w.AppendLine($"public sealed class {name}Specification : ISpecification<{name}>");
        w.OpenBrace();

        // Filter properties as nullable inputs
        for (int i = 0; i < filters.Length; i++)
        {
            var prop = filters[i];
            string nullableType = QueryFilterEmitter.GetNullableFilterType(prop);
            w.AppendLine($"public {nullableType} {prop.Name} {{ get; init; }}");
        }

        w.AppendLine("public int? Skip { get; init; }");
        w.AppendLine("public int? Take { get; init; }");
        w.AppendLine();

        // Criteria property — builds composite expression from non-null filters
        w.AppendLine($"public Expression<Func<{name}, bool>>? Criteria");
        w.OpenBrace();
        w.AppendLine("get");
        w.OpenBrace();

        w.AppendLine($"Expression<Func<{name}, bool>>? expr = null;");
        w.AppendLine();

        for (int i = 0; i < filters.Length; i++)
        {
            QueryFilterEmitter.EmitSpecificationFilter(w, name, filters[i], filters[i].Name);
        }

        w.AppendLine("return expr;");
        w.CloseBrace();
        w.CloseBrace();
        w.AppendLine();

        // Includes — empty by default, extensible
        w.AppendLine($"public IReadOnlyList<Expression<Func<{name}, object>>> Includes {{ get; init; }} = Array.Empty<Expression<Func<{name}, object>>>();");
        w.AppendLine();

        // CombineAnd helper
        w.AppendLine($"private static Expression<Func<{name}, bool>> CombineAnd(");
        w.IncreaseIndent();
        w.AppendLine($"Expression<Func<{name}, bool>> left,");
        w.AppendLine($"Expression<Func<{name}, bool>> right)");
        w.DecreaseIndent();
        w.OpenBrace();
        w.AppendLine("var parameter = Expression.Parameter(typeof({0}));".Replace("{0}", name));
        w.AppendLine("var combined = Expression.AndAlso(");
        w.IncreaseIndent();
        w.AppendLine("Expression.Invoke(left, parameter),");
        w.AppendLine("Expression.Invoke(right, parameter));");
        w.DecreaseIndent();
        w.AppendLine($"return Expression.Lambda<Func<{name}, bool>>(combined, parameter);");
        w.CloseBrace();

        w.CloseBrace();
    }
}
