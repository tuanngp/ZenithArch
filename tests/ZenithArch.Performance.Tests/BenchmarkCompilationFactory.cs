using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ZenithArch.Abstractions.Attributes;

namespace ZenithArch.Performance.Tests;

internal static class BenchmarkCompilationFactory
{
    public static CSharpCompilation CreateCompilation(int entityCount)
    {
        var source = BuildSource(entityCount);
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.Latest),
            path: "PerformanceInput.cs");

        return CSharpCompilation.Create(
            assemblyName: $"ZenithArch.Perf.{entityCount}",
            syntaxTrees: [syntaxTree],
            references: CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static IEnumerable<MetadataReference> CreateMetadataReferences()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        string? trustedAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (!string.IsNullOrWhiteSpace(trustedAssemblies))
        {
            foreach (var assemblyPath in trustedAssemblies.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                if (seen.Add(assemblyPath))
                {
                    yield return MetadataReference.CreateFromFile(assemblyPath);
                }
            }
        }

        foreach (var assembly in GetAdditionalAssemblies())
        {
            var location = assembly.Location;
            if (!string.IsNullOrWhiteSpace(location) && seen.Add(location))
            {
                yield return MetadataReference.CreateFromFile(location);
            }
        }
    }

    private static IEnumerable<Assembly> GetAdditionalAssemblies()
    {
        yield return typeof(object).Assembly;
        yield return typeof(Enumerable).Assembly;
        yield return typeof(EntityAttribute).Assembly;
        yield return typeof(ZenithArch.Generator.ZenithArchGenerator).Assembly;
    }

    private static string BuildSource(int entityCount)
    {
        var sb = new System.Text.StringBuilder(4096 + (entityCount * 256));
        sb.AppendLine("using ZenithArch.Abstractions.Attributes;");
        sb.AppendLine("using ZenithArch.Abstractions.Base;");
        sb.AppendLine("using ZenithArch.Abstractions.Enums;");
        sb.AppendLine();
        sb.AppendLine("[assembly: Architecture(");
        sb.AppendLine("    Pattern = ArchitecturePattern.Repository,");
        sb.AppendLine("    UseSpecification = true,");
        sb.AppendLine("    GenerateDependencyInjection = true)]");
        sb.AppendLine();
        sb.AppendLine("namespace Demo.Domain;");
        sb.AppendLine();

        for (int i = 0; i < entityCount; i++)
        {
            sb.AppendLine("[Entity]");
            sb.AppendLine($"public partial class Entity{i} : EntityBase");
            sb.AppendLine("{");
            sb.AppendLine("    [QueryFilter]");
            sb.AppendLine("    public string Name { get; set; } = string.Empty;");
            sb.AppendLine("    [QueryFilter]");
            sb.AppendLine("    public int Status { get; set; }");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
