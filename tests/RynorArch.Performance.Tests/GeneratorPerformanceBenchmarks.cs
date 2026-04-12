using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RynorArch.Performance.Tests;

[MemoryDiagnoser]
public class GeneratorPerformanceBenchmarks
{
    [Params(10, 50, 100)]
    public int EntityCount { get; set; }

    private CSharpCompilation _compilation = null!;
    private CSharpParseOptions _parseOptions = null!;

    [GlobalSetup]
    public void Setup()
    {
        _compilation = BenchmarkCompilationFactory.CreateCompilation(EntityCount);
        _parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
    }

    [Benchmark(Description = "Generator run result source count")]
    public int RunGenerator()
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new RynorArch.Generator.RynorArchGenerator().AsSourceGenerator()],
            parseOptions: _parseOptions);

        driver = driver.RunGenerators(_compilation);
        GeneratorDriverRunResult runResult = driver.GetRunResult();

        if (runResult.Results.Length == 0)
        {
            return 0;
        }

        return runResult.Results[0].GeneratedSources.Length;
    }
}
