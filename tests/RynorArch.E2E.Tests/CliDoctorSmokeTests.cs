using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace RynorArch.E2E.Tests;

public sealed class CliDoctorSmokeTests
{
    [Fact]
    public void Doctor_reports_not_ready_when_architecture_config_is_missing()
    {
        string repoRoot = FindRepoRoot();
        string tempDir = CreateTempProjectDirectory();

        try
        {
            var result = RunCli(repoRoot, $"doctor \"{tempDir}\"");

            Assert.NotEqual(0, result.ExitCode);
            Assert.Contains("DR004", result.Output, StringComparison.Ordinal);
            Assert.Contains("[Result] NOT READY", result.Output, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void Doctor_reports_ready_for_sample_project()
    {
        string repoRoot = FindRepoRoot();
        string samplePath = Path.Combine(repoRoot, "samples", "RynorArch.Sample");

        var result = RunCli(repoRoot, $"doctor \"{samplePath}\"");

        Assert.True(result.ExitCode == 0, $"Expected exit code 0 but got {result.ExitCode}.{Environment.NewLine}{result.Output}");
        Assert.Contains("DR004", result.Output, StringComparison.Ordinal);
        Assert.Contains("[Result] READY", result.Output, StringComparison.Ordinal);
    }

    private static (int ExitCode, string Output) RunCli(string repoRoot, string arguments)
    {
        string cliProject = Path.Combine(repoRoot, "src", "RynorArch.Cli", "RynorArch.Cli.csproj");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --framework net10.0 --project \"{cliProject}\" -- {arguments}",
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            throw new InvalidOperationException("Failed to start dotnet process for CLI smoke test.");
        }

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return (process.ExitCode, output + Environment.NewLine + error);
    }

    private static string CreateTempProjectDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), "rynorarch-e2e-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);

        File.WriteAllText(Path.Combine(path, "TempProject.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");

        File.WriteAllText(Path.Combine(path, "Trip.cs"),
            "using RynorArch.Abstractions.Attributes; [Entity] public class Trip { public string Name { get; set; } = string.Empty; }");

        return path;
    }

    private static string FindRepoRoot()
    {
        string? current = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(current))
        {
            if (File.Exists(Path.Combine(current, "RynorArch.slnx")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName;
        }

        throw new InvalidOperationException("Could not locate repository root from test execution directory.");
    }
}
