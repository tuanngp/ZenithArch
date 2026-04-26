using ZenithArch.Generator.Models;

namespace ZenithArch.Generator.Helpers;

internal static class QueryFilterEmitter
{
    public static string GetNullableFilterType(PropertyModel prop)
        => prop.IsNullable ? prop.TypeName : $"{prop.TypeName}?";

    public static void EmitQueryableFilter(SourceWriter w, PropertyModel prop, string valueAccessor)
    {
        w.AppendLine($"if ({valueAccessor} is not null)");
        w.IncreaseIndent();

        if (IsStringFilter(prop))
        {
            w.AppendLine($"queryable = queryable.Where(x => x.{prop.Name}.Contains({valueAccessor}!));");
        }
        else
        {
            w.AppendLine($"queryable = queryable.Where(x => x.{prop.Name} == {valueAccessor});");
        }

        w.DecreaseIndent();
        w.AppendLine();
    }

    public static void EmitSpecificationFilter(SourceWriter w, string entityName, PropertyModel prop, string valueAccessor)
    {
        w.AppendLine($"if ({valueAccessor} is not null)");
        w.OpenBrace();

        if (IsStringFilter(prop))
        {
            w.AppendLine($"Expression<Func<{entityName}, bool>> filter = x => x.{prop.Name}.Contains({valueAccessor}!);");
        }
        else
        {
            w.AppendLine($"Expression<Func<{entityName}, bool>> filter = x => x.{prop.Name} == {valueAccessor};");
        }

        w.AppendLine("expr = expr is null ? filter : CombineAnd(expr, filter);");
        w.CloseBrace();
        w.AppendLine();
    }

    private static bool IsStringFilter(PropertyModel prop)
        => prop.TypeName.IndexOf("string", System.StringComparison.Ordinal) >= 0;
}
