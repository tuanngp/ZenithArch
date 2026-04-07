using System;
using System.Collections.Generic;
using System.IO;

namespace RynorArch.Cli;

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
        Console.WriteLine("Welcome to Rynor Arch CLI - Productivity Framework");
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
        else
        {
            ShowHelp();
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("\nUsage: rynor init");
        Console.WriteLine("Usage: rynor init <Namespace>");
        Console.WriteLine("\nUsage: rynor scaffold");
        Console.WriteLine("Usage: rynor scaffold <EntityName> [Namespace]");
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

        string content = $@"using RynorArch.Abstractions.Attributes;
using RynorArch.Abstractions.Enums;

// Auto-generated by rynor CLI for namespace: {ns}
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

        string content = $@"using RynorArch.Abstractions.Attributes;
using RynorArch.Abstractions.Base;
using RynorArch.Abstractions.Interfaces;

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
2. Inspect generated sources in obj/ generated files and RynorArch.GenerationReport.g.cs
3. In your app startup, call: builder.Services.AddRynorArchDependencies();
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
