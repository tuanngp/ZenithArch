using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RynorArch.Generator.Helpers;

internal static class SourceEmissionHelpers
{
    public static void AddSource(SourceProductionContext context, string hintName, StringBuilder sourceBuilder)
        => context.AddSource(hintName, SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));

    public static void AddSource(SourceProductionContext context, string hintName, string source)
        => context.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
}