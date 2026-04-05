using System;
using System.IO;

namespace RynorArch.Cli;

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
        else
        {
            ShowHelp();
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("\nUsage: rynor scaffold");
        Console.WriteLine("Usage: rynor scaffold <EntityName> [Namespace]");
    }

    static void RunScaffold(string[] args)
    {
        string entityName = args.Length > 1 ? args[1] : Prompt("Entity Name (e.g. Product): ");
        if (string.IsNullOrWhiteSpace(entityName)) return;

        string ns = args.Length > 2 ? args[2] : Prompt("Namespace (e.g. MyStore.Domain): ", "MyProject.Domain");
        
        bool useSoftDelete = PromptYesNo("Enable Soft Delete? (y/N): ");
        bool useAuditable = PromptYesNo("Enable Audit Tracking? (y/N): ");

        GenerateCqrsHandlers(entityName, ns);
        GenerateDomainEntity(entityName, ns, useSoftDelete, useAuditable);
        GenerateEfConfig(entityName, ns);

        Console.WriteLine("\n🚀 [Success] Scaffolding complete!");
        Console.WriteLine($"📍 Domain: Domain/{entityName}.cs");
        Console.WriteLine($"📍 CQRS: Cqrs/{entityName}/ (5 handlers)");
        Console.WriteLine($"📍 EF: Infrastructure/Data/Configurations/{entityName}Configuration.cs");
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
    // TODO: Add your properties here
    // [QueryFilter]
    // public string Title {{ get; set; }}
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
    // TODO: Uncomment the partial methods below to implement custom logic
    
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
