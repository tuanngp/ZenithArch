using System.Collections.Immutable;
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using RynorArch.Abstractions.Attributes;

namespace RynorArch.Generator.Tests;

internal sealed record GeneratorRunResult(
    CSharpCompilation OutputCompilation,
    ImmutableArray<Diagnostic> Diagnostics,
    ImmutableDictionary<string, string> GeneratedSources);

internal static class GeneratorTestHarness
{
    public static GeneratorRunResult Run(params string[] sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        var syntaxTrees = new List<SyntaxTree>(sources.Length);
        for (int i = 0; i < sources.Length; i++)
        {
            syntaxTrees.Add(
                CSharpSyntaxTree.ParseText(
                    sources[i],
                    new CSharpParseOptions(LanguageVersion.Latest),
                    path: $"TestInput{i + 1}.cs"));
        }

        var compilation = CSharpCompilation.Create(
            assemblyName: $"RynorArch.Tests.{Guid.NewGuid():N}",
            syntaxTrees: syntaxTrees,
            references: CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new RynorArchGenerator().AsSourceGenerator()],
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics);

        var runResult = driver.GetRunResult();
        var allDiagnostics = outputCompilation.GetDiagnostics()
            .AddRange(generatorDiagnostics)
            .AddRange(runResult.Diagnostics);

        var generatedSources = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach (var result in runResult.Results)
        {
            foreach (var generatedSource in result.GeneratedSources)
            {
                generatedSources[generatedSource.HintName] = generatedSource.SourceText.ToString();
            }
        }

        return new GeneratorRunResult(
            (CSharpCompilation)outputCompilation,
            allDiagnostics,
            generatedSources.ToImmutable());
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
        yield return typeof(DbContext).Assembly;
        yield return typeof(IMediator).Assembly;
        yield return typeof(AbstractValidator<>).Assembly;
    }
}
