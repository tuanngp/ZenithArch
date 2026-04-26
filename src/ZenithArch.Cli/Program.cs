using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ZenithArch.Cli;

internal enum CliArchitecturePattern
{
    Cqrs,
    Repository,
    FullStack,
}

internal sealed class CliScaffoldOptions
{
    public CliArchitecturePattern Pattern { get; set; }
    public bool GenerateEndpoints { get; set; }
    public bool EnableExperimentalEndpoints { get; set; }
    public bool GenerateCachingDecorators { get; set; }
    public bool UsePerRequestSaveMode { get; set; }
}

internal enum DoctorSeverity
{
    Pass,
    Warn,
    Fail,
}

internal sealed class DoctorCheckResult
{
    public string Id { get; }
    public DoctorSeverity Severity { get; }
    public string Check { get; }
    public string Message { get; }
    public string Fix { get; }

    public DoctorCheckResult(string id, DoctorSeverity severity, string check, string message, string fix = "")
    {
        Id = id;
        Severity = severity;
        Check = check;
        Message = message;
        Fix = fix;
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(@"
██████╗ ██╗   ██╗███╗   ██╗ ██████╗ ██████╗      █████╗ ██████╗  ██████╗██╗  ██╗
██╔══██╗╚██╗ ██╔╝████╗  ██║██╔═══██╗██╔══██╗    ██╔══██╗██╔══██╗██╔════╝██║  ██║
██████╔╝ ╚████╔╝ ██╔██╗ ██║██║   ██║██████╔╝    ███████║██████╔╝██║     ███████║
██╔══██╗  ╚██╔╝  ██║╚██╗██║██║   ██║██╔══██╗    ██╔══██║██╔══██╗██║     ██╔══██║
██║  ██║   ██║   ██║ ╚████║╚██████╔╝██║  ██║    ██║  ██║██║  ██║╚██████╗██║  ██║
╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═══╝ ╚═════╝ ╚═╝  ╚═╝    ╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝
");
        Console.WriteLine("Welcome to Zenith Arch CLI - Productivity Framework");
        Console.WriteLine("-----------------------------------------------");

        string command = args.Length > 0 ? args[0].ToLowerInvariant() : "";

        if (command == "scaffold")
        {
            RunScaffold(args);
        }
        else if (command == "init")
        {
            RunInit(args);
        }
        else if (command == "doctor")
        {
            RunDoctor(args);
        }
        else
        {
            ShowHelp();
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("\nUsage: zenith init");
        Console.WriteLine("Usage: zenith init <Namespace>");
        Console.WriteLine("\nUsage: zenith scaffold");
        Console.WriteLine("Usage: zenith scaffold <EntityName> [Namespace]");
        Console.WriteLine("\nUsage: zenith doctor");
        Console.WriteLine("Usage: zenith doctor <ProjectPath>");
    }

    static void RunInit(string[] args)
    {
        string ns = args.Length > 1 ? args[1] : Prompt("Namespace (e.g. MyStore.Domain): ", "MyProject.Domain");
        var options = PromptArchitectureOptions();

        GenerateAssemblyConfig(ns, options);
        GenerateNextSteps(options, null, ns);

        Console.WriteLine("\n[Success] Initialization complete");
        Console.WriteLine("[Created] AssemblyConfig.cs");
        Console.WriteLine("[Created] README_NEXT_STEPS.md");
    }

    static void RunScaffold(string[] args)
    {
        string entityName = args.Length > 1 ? args[1] : Prompt("Entity Name (e.g. Product): ");
        if (string.IsNullOrWhiteSpace(entityName)) return;

        string ns = args.Length > 2 ? args[2] : Prompt("Namespace (e.g. MyStore.Domain): ", "MyProject.Domain");
        var options = PromptArchitectureOptions();
        
        bool useSoftDelete = PromptYesNo("Enable Soft Delete? (y/N): ");
        bool useAuditable = PromptYesNo("Enable Audit Tracking? (y/N): ");

        GenerateAssemblyConfig(ns, options);
        GenerateDomainEntity(entityName, ns, useSoftDelete, useAuditable);

        if (options.Pattern != CliArchitecturePattern.Repository)
        {
            GenerateCqrsHandlers(entityName, ns);
        }

        if (options.Pattern != CliArchitecturePattern.Cqrs || PromptYesNo("Generate EF configuration partial file? (Y/n): "))
        {
            GenerateEfConfig(entityName, ns);
        }

        GenerateNextSteps(options, entityName, ns);

        Console.WriteLine("\n[Success] Scaffolding complete");
        Console.WriteLine($"[Created] Domain/{entityName}.cs");
        if (options.Pattern != CliArchitecturePattern.Repository)
        {
            Console.WriteLine($"[Created] Cqrs/{entityName}/ (5 handler extension files)");
        }
        Console.WriteLine($"[Created] AssemblyConfig.cs");
        Console.WriteLine($"[Created] README_NEXT_STEPS.md");
    }

    static void RunDoctor(string[] args)
    {
        string targetPath = args.Length > 1 ? args[1] : Directory.GetCurrentDirectory();
        targetPath = Path.GetFullPath(targetPath);

        Console.WriteLine($"\n[Info] Running doctor checks in: {targetPath}");

        var results = new List<DoctorCheckResult>();
        if (!Directory.Exists(targetPath))
        {
            results.Add(new DoctorCheckResult(
                "DR000",
                DoctorSeverity.Fail,
                "Project root",
                $"Directory not found: {targetPath}",
                "Run zenith doctor from your project root or pass a valid path."));
            PrintDoctorSummary(results);
            Environment.ExitCode = 1;
            return;
        }

        CheckDotnetSdk(results);

        string? projectFilePath = ResolveProjectFile(targetPath, results);
        var packageReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var frameworkReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var projectReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrEmpty(projectFilePath))
        {
            LoadProjectReferences(projectFilePath, packageReferences, frameworkReferences, projectReferences, results);
        }

        string? architectureConfigPath = ResolveArchitectureConfigPath(targetPath, results);
        string architectureConfigContent = string.Empty;
        if (!string.IsNullOrEmpty(architectureConfigPath))
        {
            architectureConfigContent = File.ReadAllText(architectureConfigPath);
            CheckArchitectureConfigConsistency(architectureConfigContent, results);
        }

        ValidateDependencies(packageReferences, frameworkReferences, projectReferences, architectureConfigContent, results);
        CheckEntityPartialDeclarations(targetPath, results);
        CheckGenerationReport(targetPath, results);

        PrintDoctorSummary(results);
        if (HasSeverity(results, DoctorSeverity.Fail))
        {
            Environment.ExitCode = 1;
        }
    }

    static void CheckDotnetSdk(List<DoctorCheckResult> results)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                results.Add(new DoctorCheckResult("DR001", DoctorSeverity.Fail, "dotnet SDK", "Unable to run dotnet --version", "Install .NET SDK and ensure dotnet is available in PATH."));
                return;
            }

            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
            {
                results.Add(new DoctorCheckResult("DR001", DoctorSeverity.Fail, "dotnet SDK", "dotnet --version did not return a valid SDK version", "Install .NET SDK 10.0.x for the recommended environment."));
                return;
            }

            if (output.StartsWith("10.", StringComparison.Ordinal))
            {
                results.Add(new DoctorCheckResult("DR001", DoctorSeverity.Pass, "dotnet SDK", $"Detected SDK version {output}"));
            }
            else
            {
                results.Add(new DoctorCheckResult("DR001", DoctorSeverity.Warn, "dotnet SDK", $"Detected SDK version {output}", "Use .NET SDK 10.0.x for the validated support matrix."));
            }
        }
        catch (Exception ex)
        {
            results.Add(new DoctorCheckResult("DR001", DoctorSeverity.Fail, "dotnet SDK", $"Failed to detect SDK version: {ex.Message}", "Install .NET SDK and confirm dotnet is available in PATH."));
        }
    }

    static string? ResolveProjectFile(string targetPath, List<DoctorCheckResult> results)
    {
        string[] topLevel = Directory.GetFiles(targetPath, "*.csproj", SearchOption.TopDirectoryOnly);
        string[] all = topLevel;
        if (all.Length == 0)
        {
            all = Directory.GetFiles(targetPath, "*.csproj", SearchOption.AllDirectories);
        }

        if (all.Length == 0)
        {
            results.Add(new DoctorCheckResult("DR002", DoctorSeverity.Fail, "Project file", "No .csproj file was found", "Run zenith doctor from a .NET project root or pass a valid project folder."));
            return null;
        }

        Array.Sort(all, StringComparer.OrdinalIgnoreCase);
        string selected = all[0];

        if (all.Length > 1)
        {
            results.Add(new DoctorCheckResult("DR002", DoctorSeverity.Warn, "Project file", $"Multiple .csproj files found, using {Path.GetFileName(selected)}", "Pass an explicit project path to avoid ambiguity."));
        }
        else
        {
            results.Add(new DoctorCheckResult("DR002", DoctorSeverity.Pass, "Project file", $"Using {Path.GetFileName(selected)}"));
        }

        return selected;
    }

    static void LoadProjectReferences(
        string projectFilePath,
        HashSet<string> packageReferences,
        HashSet<string> frameworkReferences,
        HashSet<string> projectReferences,
        List<DoctorCheckResult> results)
    {
        try
        {
            var document = XDocument.Load(projectFilePath);
            foreach (var element in document.Descendants())
            {
                string localName = element.Name.LocalName;

                if (localName == "PackageReference")
                {
                    string? include = element.Attribute("Include")?.Value;
                    if (!string.IsNullOrWhiteSpace(include))
                    {
                        packageReferences.Add(include);
                    }
                }
                else if (localName == "FrameworkReference")
                {
                    string? include = element.Attribute("Include")?.Value;
                    if (!string.IsNullOrWhiteSpace(include))
                    {
                        frameworkReferences.Add(include);
                    }
                }
                else if (localName == "ProjectReference")
                {
                    string? include = element.Attribute("Include")?.Value;
                    if (!string.IsNullOrWhiteSpace(include))
                    {
                        string normalizedInclude = include
                            .Replace('\\', Path.DirectorySeparatorChar)
                            .Replace('/', Path.DirectorySeparatorChar);

                        string projectName = Path.GetFileNameWithoutExtension(normalizedInclude);
                        if (!string.IsNullOrWhiteSpace(projectName))
                        {
                            projectReferences.Add(projectName);
                        }
                    }
                }
            }

            results.Add(new DoctorCheckResult("DR003", DoctorSeverity.Pass, "Project references", "Loaded package/framework/project references"));
        }
        catch (Exception ex)
        {
            results.Add(new DoctorCheckResult("DR003", DoctorSeverity.Fail, "Project references", $"Failed to parse .csproj: {ex.Message}", "Fix project XML syntax and rerun doctor."));
        }
    }

    static string? ResolveArchitectureConfigPath(string targetPath, List<DoctorCheckResult> results)
    {
        string assemblyConfigPath = Path.Combine(targetPath, "AssemblyConfig.cs");
        if (File.Exists(assemblyConfigPath))
        {
            results.Add(new DoctorCheckResult("DR004", DoctorSeverity.Pass, "Architecture config", "Found AssemblyConfig.cs"));
            return assemblyConfigPath;
        }

        foreach (string file in EnumerateSourceFiles(targetPath))
        {
            string content = File.ReadAllText(file);
            if (content.Contains("[assembly: Architecture(", StringComparison.Ordinal))
            {
                results.Add(new DoctorCheckResult("DR004", DoctorSeverity.Warn, "Architecture config", $"Found assembly Architecture attribute in {Path.GetFileName(file)}", "Prefer a dedicated AssemblyConfig.cs at project root for discoverability."));
                return file;
            }
        }

        results.Add(new DoctorCheckResult("DR004", DoctorSeverity.Fail, "Architecture config", "No [assembly: Architecture(...)] configuration found", "Run zenith init or add AssemblyConfig.cs with [assembly: Architecture(...)]."));
        return null;
    }

    static void CheckArchitectureConfigConsistency(string content, List<DoctorCheckResult> results)
    {
        if (!content.Contains("[assembly: Architecture(", StringComparison.Ordinal))
        {
            results.Add(new DoctorCheckResult("DR005", DoctorSeverity.Fail, "Architecture declaration", "Assembly config file exists but Architecture attribute is missing", "Add [assembly: Architecture(...)] in AssemblyConfig.cs."));
            return;
        }

        bool hasProfile = content.Contains("Profile = ArchitectureProfile.", StringComparison.Ordinal);
        bool generateEndpoints = content.Contains("GenerateEndpoints = true", StringComparison.Ordinal);
        bool enableExperimentalEndpoints = content.Contains("EnableExperimentalEndpoints = true", StringComparison.Ordinal);
        bool hasEndpointHardeningMode = content.Contains("EndpointHardeningMode =", StringComparison.Ordinal);
        bool endpointHardeningModeNone = content.Contains("EndpointHardeningMode = EndpointHardeningMode.None", StringComparison.Ordinal);

        if (!hasProfile)
        {
            results.Add(new DoctorCheckResult("DR005", DoctorSeverity.Warn, "Architecture declaration", "Configuration does not declare a starter profile", "Use Profile = ArchitectureProfile.CqrsQuickStart/RepositoryQuickStart/FullStackQuickStart to reduce config drift."));
        }
        else
        {
            results.Add(new DoctorCheckResult("DR005", DoctorSeverity.Pass, "Architecture declaration", "Starter profile detected"));
        }

        if (generateEndpoints && !enableExperimentalEndpoints)
        {
            results.Add(new DoctorCheckResult("DR006", DoctorSeverity.Fail, "Endpoint opt-in", "GenerateEndpoints is enabled without EnableExperimentalEndpoints", "Set EnableExperimentalEndpoints = true or disable GenerateEndpoints."));
        }
        else if (generateEndpoints)
        {
            results.Add(new DoctorCheckResult("DR006", DoctorSeverity.Pass, "Endpoint opt-in", "Endpoint generation has explicit experimental opt-in"));
            if (hasEndpointHardeningMode && !endpointHardeningModeNone)
            {
                results.Add(new DoctorCheckResult("DR016", DoctorSeverity.Pass, "Endpoint hardening", "Endpoint hardening mode is enabled"));
            }
            else
            {
                results.Add(new DoctorCheckResult("DR016", DoctorSeverity.Warn, "Endpoint hardening", "Generated endpoints are intentionally minimal", "Apply docs/ENDPOINT_HARDENING.md before production rollout."));
            }
        }

        if (hasEndpointHardeningMode && !endpointHardeningModeNone)
        {
            if (!generateEndpoints)
            {
                results.Add(new DoctorCheckResult("DR017", DoctorSeverity.Warn, "Endpoint hardening mode", "EndpointHardeningMode is configured while GenerateEndpoints is disabled", "Enable GenerateEndpoints + EnableExperimentalEndpoints, or set EndpointHardeningMode = EndpointHardeningMode.None."));
            }
            else if (!enableExperimentalEndpoints)
            {
                results.Add(new DoctorCheckResult("DR017", DoctorSeverity.Fail, "Endpoint hardening mode", "EndpointHardeningMode requires EnableExperimentalEndpoints = true", "Set EnableExperimentalEndpoints = true or reset EndpointHardeningMode to None."));
            }
            else
            {
                results.Add(new DoctorCheckResult("DR017", DoctorSeverity.Pass, "Endpoint hardening mode", "Endpoint hardening mode is configured coherently"));
            }
        }
    }

    static void ValidateDependencies(
        HashSet<string> packageReferences,
        HashSet<string> frameworkReferences,
        HashSet<string> projectReferences,
        string architectureConfigContent,
        List<DoctorCheckResult> results)
    {
        bool isCqrs = HasAny(architectureConfigContent,
            "ArchitecturePattern.Cqrs",
            "ArchitecturePattern.FullStack",
            "ArchitectureProfile.CqrsQuickStart",
            "ArchitectureProfile.FullStackQuickStart");

        bool isRepository = HasAny(architectureConfigContent,
            "ArchitecturePattern.Repository",
            "ArchitecturePattern.FullStack",
            "ArchitectureProfile.RepositoryQuickStart",
            "ArchitectureProfile.FullStackQuickStart");

        bool enableValidation = HasAny(architectureConfigContent,
            "EnableValidation = true",
            "ArchitectureProfile.CqrsQuickStart",
            "ArchitectureProfile.FullStackQuickStart");

        bool generateCaching = architectureConfigContent.Contains("GenerateCachingDecorators = true", StringComparison.Ordinal);
        bool generateEndpoints = architectureConfigContent.Contains("GenerateEndpoints = true", StringComparison.Ordinal);
        bool needsPersistence = isCqrs || isRepository || architectureConfigContent.Contains("GenerateEfConfigurations = true", StringComparison.Ordinal);

        bool hasAbstractions = packageReferences.Contains("ZenithArch.Abstractions") || projectReferences.Contains("ZenithArch.Abstractions");
        bool hasGenerator = packageReferences.Contains("ZenithArch.Generator") || projectReferences.Contains("ZenithArch.Generator");

        AddDependencyResult(results, "DR007", "ZenithArch.Abstractions", hasAbstractions, "Add ZenithArch.Abstractions package (or project reference).");
        AddDependencyResult(results, "DR008", "ZenithArch.Generator", hasGenerator, "Add ZenithArch.Generator as analyzer package (or project reference).", true);

        if (isCqrs)
        {
            AddDependencyResult(results, "DR009", "MediatR", packageReferences.Contains("MediatR"), "Add <PackageReference Include=\"MediatR\" Version=\"12.*\" />.");
        }

        if (enableValidation)
        {
            AddDependencyResult(results, "DR010", "FluentValidation", packageReferences.Contains("FluentValidation"), "Add <PackageReference Include=\"FluentValidation\" Version=\"11.*\" />.");
        }

        if (needsPersistence)
        {
            AddDependencyResult(results, "DR011", "Microsoft.EntityFrameworkCore", HasPackagePrefix(packageReferences, "Microsoft.EntityFrameworkCore"), "Add <PackageReference Include=\"Microsoft.EntityFrameworkCore\" Version=\"9.*\" />.");
        }

        if (generateCaching)
        {
            AddDependencyResult(results, "DR012", "Microsoft.Extensions.Caching.*", HasPackagePrefix(packageReferences, "Microsoft.Extensions.Caching"), "Add a distributed cache package (for example Microsoft.Extensions.Caching.StackExchangeRedis).", warnOnly: true);
        }

        if (generateEndpoints)
        {
            AddDependencyResult(results, "DR013", "Microsoft.AspNetCore.App", frameworkReferences.Contains("Microsoft.AspNetCore.App"), "Add <FrameworkReference Include=\"Microsoft.AspNetCore.App\" />.");
        }
    }

    static void CheckEntityPartialDeclarations(string targetPath, List<DoctorCheckResult> results)
    {
        int entityDeclarations = 0;
        int nonPartialDeclarations = 0;
        var regex = new Regex(@"\[Entity\][\s\S]{0,600}?class\s+[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);

        foreach (string file in EnumerateSourceFiles(targetPath))
        {
            string content = File.ReadAllText(file);
            var matches = regex.Matches(content);
            for (int i = 0; i < matches.Count; i++)
            {
                entityDeclarations++;
                if (!matches[i].Value.Contains("partial class", StringComparison.OrdinalIgnoreCase))
                {
                    nonPartialDeclarations++;
                }
            }
        }

        if (entityDeclarations == 0)
        {
            results.Add(new DoctorCheckResult("DR014", DoctorSeverity.Warn, "Entity declarations", "No [Entity] declarations were found", "Run zenith scaffold to create an entity or add [Entity] to a partial class."));
            return;
        }

        if (nonPartialDeclarations > 0)
        {
            results.Add(new DoctorCheckResult("DR014", DoctorSeverity.Fail, "Entity declarations", $"Found {nonPartialDeclarations} non-partial [Entity] declaration(s)", "Mark all [Entity] classes as partial to enable generation."));
            return;
        }

        results.Add(new DoctorCheckResult("DR014", DoctorSeverity.Pass, "Entity declarations", $"Validated {entityDeclarations} [Entity] declaration(s) as partial"));
    }

    static void CheckGenerationReport(string targetPath, List<DoctorCheckResult> results)
    {
        string[] reports = Directory.GetFiles(targetPath, "ZenithArch.GenerationReport.g.cs", SearchOption.AllDirectories);
        for (int i = 0; i < reports.Length; i++)
        {
            if (reports[i].Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(new DoctorCheckResult("DR015", DoctorSeverity.Pass, "Generated artifacts", "Found ZenithArch.GenerationReport.g.cs"));
                return;
            }
        }

        results.Add(new DoctorCheckResult("DR015", DoctorSeverity.Warn, "Generated artifacts", "Generation report not found under obj/", "Run dotnet build once, then rerun zenith doctor."));
    }

    static IEnumerable<string> EnumerateSourceFiles(string root)
    {
        foreach (string file in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || file.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || file.Contains(Path.DirectorySeparatorChar + "Generated" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            yield return file;
        }
    }

    static void AddDependencyResult(
        List<DoctorCheckResult> results,
        string id,
        string dependencyName,
        bool isPresent,
        string fix,
        bool warnOnly = false)
    {
        if (isPresent)
        {
            results.Add(new DoctorCheckResult(id, DoctorSeverity.Pass, $"Dependency {dependencyName}", "Present"));
            return;
        }

        results.Add(new DoctorCheckResult(
            id,
            warnOnly ? DoctorSeverity.Warn : DoctorSeverity.Fail,
            $"Dependency {dependencyName}",
            "Missing",
            fix));
    }

    static bool HasPackagePrefix(HashSet<string> packageReferences, string prefix)
    {
        foreach (string reference in packageReferences)
        {
            if (reference.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    static bool HasAny(string value, params string[] tokens)
    {
        for (int i = 0; i < tokens.Length; i++)
        {
            if (value.Contains(tokens[i], StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    static bool HasSeverity(List<DoctorCheckResult> results, DoctorSeverity severity)
    {
        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].Severity == severity)
            {
                return true;
            }
        }

        return false;
    }

    static int CountSeverity(List<DoctorCheckResult> results, DoctorSeverity severity)
    {
        int count = 0;
        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].Severity == severity)
            {
                count++;
            }
        }

        return count;
    }

    static void PrintDoctorSummary(List<DoctorCheckResult> results)
    {
        Console.WriteLine();
        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];
            Console.WriteLine($"[{result.Severity.ToString().ToUpperInvariant()}] {result.Id} {result.Check}: {result.Message}");
            if (!string.IsNullOrWhiteSpace(result.Fix) && result.Severity != DoctorSeverity.Pass)
            {
                Console.WriteLine($"       Fix: {result.Fix}");
            }
        }

        int pass = CountSeverity(results, DoctorSeverity.Pass);
        int warn = CountSeverity(results, DoctorSeverity.Warn);
        int fail = CountSeverity(results, DoctorSeverity.Fail);

        Console.WriteLine();
        Console.WriteLine($"[Summary] PASS={pass} WARN={warn} FAIL={fail}");

        if (fail > 0)
        {
            Console.WriteLine("[Result] NOT READY - resolve FAIL checks, then rerun zenith doctor");
            return;
        }

        if (warn > 0)
        {
            Console.WriteLine("[Result] READY WITH WARNINGS - recommended to resolve WARN checks");
            return;
        }

        Console.WriteLine("[Result] READY - project is aligned with the recommended setup");
    }

    static CliScaffoldOptions PromptArchitectureOptions()
    {
        Console.WriteLine("\nSelect architecture profile:");
        Console.WriteLine("  1) CqrsQuickStart");
        Console.WriteLine("  2) RepositoryQuickStart");
        Console.WriteLine("  3) FullStackQuickStart");

        string selected = Prompt("Profile [1]: ", "1");
        var options = new CliScaffoldOptions();

        options.Pattern = selected switch
        {
            "2" => CliArchitecturePattern.Repository,
            "3" => CliArchitecturePattern.FullStack,
            _ => CliArchitecturePattern.Cqrs,
        };

        if (options.Pattern != CliArchitecturePattern.Repository)
        {
            options.GenerateEndpoints = PromptYesNo("Enable generated endpoints (experimental)? (y/N): ");
            options.EnableExperimentalEndpoints = options.GenerateEndpoints;
            options.GenerateCachingDecorators = PromptYesNo("Enable query caching decorators? (y/N): ");
            options.UsePerRequestSaveMode = PromptYesNo("Use per-request transactional save mode? (y/N): ");
        }

        return options;
    }

    static void GenerateAssemblyConfig(string ns, CliScaffoldOptions options)
    {
        var profileName = options.Pattern switch
        {
            CliArchitecturePattern.Repository => "ArchitectureProfile.RepositoryQuickStart",
            CliArchitecturePattern.FullStack => "ArchitectureProfile.FullStackQuickStart",
            _ => "ArchitectureProfile.CqrsQuickStart",
        };

        var patternName = options.Pattern switch
        {
            CliArchitecturePattern.Repository => "ArchitecturePattern.Repository",
            CliArchitecturePattern.FullStack => "ArchitecturePattern.FullStack",
            _ => "ArchitecturePattern.Cqrs",
        };

        var args = new List<string>
        {
            $"    Profile = {profileName}",
            $"    Pattern = {patternName}",
            "    GenerateDependencyInjection = true",
        };

        if (options.GenerateEndpoints)
        {
            args.Add("    GenerateEndpoints = true");
            args.Add("    EnableExperimentalEndpoints = true");
        }

        if (options.GenerateCachingDecorators)
        {
            args.Add("    GenerateCachingDecorators = true");
        }

        if (options.UsePerRequestSaveMode && options.Pattern != CliArchitecturePattern.Repository)
        {
            args.Add("    CqrsSaveMode = CqrsSaveMode.PerRequestTransaction");
        }

        string content = $@"using ZenithArch.Abstractions.Attributes;
using ZenithArch.Abstractions.Enums;

// Auto-generated by zenith CLI for namespace: {ns}
[assembly: Architecture(
{string.Join(",\n", args)}
)]
";

        string path = Path.Combine(Directory.GetCurrentDirectory(), "AssemblyConfig.cs");
        WriteFile(path, content);
    }

    static void GenerateDomainEntity(string name, string ns, bool softDelete, bool auditable)
    {
        string interfaces = "";
        string properties = "";

        if (softDelete)
        {
            interfaces += ", ISoftDelete";
            properties += "    public bool IsDeleted { get; set; }\n";
        }
        if (auditable)
        {
            interfaces += ", IAuditable";
            properties += "    public DateTime CreatedAt { get; set; }\n";
            properties += "    public string? CreatedBy { get; set; }\n";
            properties += "    public DateTime? LastModifiedAt { get; set; }\n";
            properties += "    public string? LastModifiedBy { get; set; }\n";
        }

        string content = $@"using ZenithArch.Abstractions.Attributes;
using ZenithArch.Abstractions.Base;
using ZenithArch.Abstractions.Interfaces;

namespace {ns};

[Entity]
[AggregateRoot]
public partial class {name} : EntityBase{interfaces}
{{
{properties}
    // TODO: Add your business properties
    [QueryFilter]
    public string Name {{ get; set; }} = string.Empty;

    [QueryFilter]
    public DateTime CreatedDate {{ get; set; }}
}}
";
        string dir = Path.Combine(Directory.GetCurrentDirectory(), "Domain");
        Directory.CreateDirectory(dir);
        WriteFile(Path.Combine(dir, $"{name}.cs"), content);
    }

    static void GenerateCqrsHandlers(string entityName, string ns)
    {
        var templates = new (string ClassPattern, string MethodSignatures)[]
        {
            ("Create{0}Handler", "partial void OnValidate(Create{0}Command command);\n    partial void OnBeforeHandle(Create{0}Command command, {0} entity);"),
            ("Update{0}Handler", "partial void OnValidate(Update{0}Command command);\n    partial void OnBeforeHandle(Update{0}Command command, {0} entity);"),
            ("Delete{0}Handler", "partial void OnBeforeHandle(Delete{0}Command command, {0}? entity);"),
            ("Get{0}ByIdHandler", "partial void OnAfterHandle({0}? entity);"),
            ("Get{0}ListHandler", "partial void OnBeforeQuery(Get{0}ListQuery query, ref System.Linq.IQueryable<{0}> queryable);")
        };

        string targetDir = Path.Combine(Directory.GetCurrentDirectory(), "Cqrs", entityName);
        Directory.CreateDirectory(targetDir);

        foreach (var t in templates)
        {
            string className = string.Format(t.ClassPattern, entityName);
            string methods = string.Format(t.MethodSignatures, entityName);
            
            string content = $@"namespace {ns}.Cqrs;

public sealed partial class {className}
{{
    // Suggested extension point. Keep only what your use case needs.
    // {methods.Replace("\n", "\n    // ")}
}}
";
            WriteFile(Path.Combine(targetDir, $"{className}.cs"), content);
        }
    }

    static void GenerateEfConfig(string entityName, string ns)
    {
        string content = $@"using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace {ns}.Infrastructure.Data.Configurations;

public partial class {entityName}Configuration
{{
    // partial void ConfigurePartial(EntityTypeBuilder<{entityName}> builder)
    // {{
    //     // builder.HasIndex(x => x.Name).IsUnique();
    // }}
}}
";
        string efConfigDir = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Data", "Configurations");
        Directory.CreateDirectory(efConfigDir);
        WriteFile(Path.Combine(efConfigDir, $"{entityName}Configuration.cs"), content);
    }

    static void GenerateNextSteps(CliScaffoldOptions options, string? entityName, string ns)
    {
        string handlerLine = options.Pattern == CliArchitecturePattern.Repository
            ? "4. Inject generated repositories and start using I{Entity}Repository contracts"
            : "4. Optionally extend generated partial handlers in Cqrs/{Entity} to add custom logic";

        if (!string.IsNullOrEmpty(entityName))
        {
            handlerLine = handlerLine.Replace("{Entity}", entityName);
        }

        string content = $@"# Next Steps

1. Run: dotnet build
2. Inspect generated sources in obj/ generated files and ZenithArch.GenerationReport.g.cs
3. In your app startup, call: builder.Services.AddZenithArchDependencies();
{handlerLine}
5. Namespace configured: {ns}
";

        string path = Path.Combine(Directory.GetCurrentDirectory(), "README_NEXT_STEPS.md");
        WriteFile(path, content);
    }

    static string Prompt(string message, string defaultValue = "")
    {
        Console.Write(message);
        if (!string.IsNullOrEmpty(defaultValue)) Console.Write($"[{defaultValue}] ");
        string? input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? defaultValue : input;
    }

    static bool PromptYesNo(string message)
    {
        Console.Write(message);
        string? input = Console.ReadLine()?.ToLowerInvariant();
        return input == "y" || input == "yes";
    }

    static void WriteFile(string path, string content)
    {
        if (!File.Exists(path))
        {
            File.WriteAllText(path, content);
            Console.WriteLine($"[Created] {path}");
        }
        else
        {
            Console.WriteLine($"[Skipped] {path} (Exists)");
        }
    }
}
