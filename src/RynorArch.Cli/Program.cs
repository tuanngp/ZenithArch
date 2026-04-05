using System;
using System.IO;

namespace RynorArch.Cli;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("RynorArch CLI Tool");
            Console.WriteLine("------------------");
            Console.WriteLine("Scaffolds partial classes for manual implementation.");
            Console.WriteLine("\nUsage: rynor scaffold <EntityName> [Namespace]");
            Console.WriteLine("Example: rynor scaffold Trip RynorArch.Sample.Cqrs");
            return;
        }

        string command = args[0].ToLowerInvariant();
        if (command != "scaffold")
        {
            Console.WriteLine("Invalid command. Please use the 'scaffold' command.");
            return;
        }

        string entityName = args[1];
        string ns = args.Length > 2 ? args[2] : "MyProject.Cqrs";
        
        // Pre-defined templates for necessary Hooks
        var templates = new (string ClassPattern, string MethodSignatures)[]
        {
            ("Create{0}Handler", "partial void OnValidate(Create{0}Command command);\n    partial void OnBeforeHandle(Create{0}Command command, {0} entity);"),
            ("Update{0}Handler", "partial void OnValidate(Update{0}Command command);\n    partial void OnBeforeHandle(Update{0}Command command, {0} entity);"),
            ("Delete{0}Handler", "partial void OnBeforeHandle(Delete{0}Command command, {0}? entity);"),
            ("Get{0}ByIdHandler", "partial void OnAfterHandle({0}? entity);"),
            ("Get{0}ListHandler", "partial void OnBeforeQuery(Get{0}ListQuery query, ref System.Linq.IQueryable<{0}> queryable);")
        };

        // Write files into the Cqrs/{EntityName} directory
        string targetDir = Path.Combine(Directory.GetCurrentDirectory(), "Cqrs", entityName);
        Directory.CreateDirectory(targetDir);

        foreach (var t in templates)
        {
            string className = string.Format(t.ClassPattern, entityName);
            string methods = string.Format(t.MethodSignatures, entityName);
            
            string content = $@"namespace {ns};

public sealed partial class {className}
{{
    // TODO: Uncomment the partial methods below to implement custom logic
    
    // {methods.Replace("\n", "\n    // ")}
}}
";
            string filePath = Path.Combine(targetDir, $"{className}.cs");
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, content);
                Console.WriteLine($"[Created] {filePath}");
            }
            else
            {
                Console.WriteLine($"[Skipped] {filePath} (File already exists)");
            }
        }
        
        // EF Core Configuration
        string efConfigContent = $@"using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace {ns}.Infrastructure.Data.Configurations;

public partial class {entityName}Configuration
{{
    // TODO: Uncomment the partial method below to configure Database Constraints
    
    // partial void ConfigurePartial(EntityTypeBuilder<{entityName}> builder)
    // {{
    //     // builder.HasIndex(x => x.Name).IsUnique();
    // }}
}}
";
        string efConfigDir = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Data", "Configurations");
        Directory.CreateDirectory(efConfigDir);
        string efConfigPath = Path.Combine(efConfigDir, $"{entityName}Configuration.cs");
        if (!File.Exists(efConfigPath))
        {
            File.WriteAllText(efConfigPath, efConfigContent);
            Console.WriteLine($"[Created] {efConfigPath}");
        }

        Console.WriteLine("\n[RYNOR] Physical layout scaffolded successfully! You can now open your IDE to code manually.");
    }
}
