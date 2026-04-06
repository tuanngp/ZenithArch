using Microsoft.CodeAnalysis;
using Xunit;

namespace RynorArch.Generator.Tests;

public sealed class GeneratorDiagnosticsTests
{
    [Fact]
    public void Reports_warning_when_no_entities_are_declared()
    {
        var result = GeneratorTestHarness.Run("""
            using RynorArch.Abstractions.Attributes;
            using RynorArch.Abstractions.Enums;

            [assembly: Architecture(Pattern = ArchitecturePattern.Cqrs)]

            namespace Demo;

            public sealed class Placeholder;
            """);

        AssertContainsDiagnostic(result.Diagnostics, "RYNOR001", DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Reports_error_when_aggregate_root_is_missing_entity_attribute()
    {
        var result = GeneratorTestHarness.Run("""
            using RynorArch.Abstractions.Attributes;

            namespace Demo.Domain;

            [AggregateRoot]
            public sealed partial class Trip
            {
            }
            """);

        AssertContainsDiagnostic(result.Diagnostics, "RYNOR002", DiagnosticSeverity.Error);
    }

    [Fact]
    public void Reports_error_when_entity_is_not_partial()
    {
        var result = GeneratorTestHarness.Run("""
            using RynorArch.Abstractions.Attributes;
            using RynorArch.Abstractions.Base;
            using RynorArch.Abstractions.Enums;

            [assembly: Architecture(Pattern = ArchitecturePattern.Repository)]

            namespace Demo.Domain;

            [Entity]
            public class Trip : EntityBase
            {
            }
            """);

        AssertContainsDiagnostic(result.Diagnostics, "RYNOR005", DiagnosticSeverity.Error);
    }

    [Fact]
    public void Reports_warning_for_unsupported_query_filter_type()
    {
        var result = GeneratorTestHarness.Run("""
            using RynorArch.Abstractions.Attributes;
            using RynorArch.Abstractions.Base;
            using RynorArch.Abstractions.Enums;

            [assembly: Architecture(Pattern = ArchitecturePattern.Repository, UseSpecification = true)]

            namespace Demo.Domain;

            [Entity]
            public partial class Trip : EntityBase
            {
                [QueryFilter]
                public object? Metadata { get; init; }
            }
            """);

        AssertContainsDiagnostic(result.Diagnostics, "RYNOR004", DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Reports_error_when_architecture_configuration_is_missing()
    {
        var result = GeneratorTestHarness.Run("""
            using RynorArch.Abstractions.Attributes;
            using RynorArch.Abstractions.Base;

            namespace Demo.Domain;

            [Entity]
            public partial class Trip : EntityBase
            {
            }
            """);

        AssertContainsDiagnostic(result.Diagnostics, "RYNOR006", DiagnosticSeverity.Error);
    }

    [Fact]
    public void Reports_error_when_configured_dbcontext_type_is_not_derived_from_dbcontext()
    {
        var result = GeneratorTestHarness.Run("""
            using RynorArch.Abstractions.Attributes;
            using RynorArch.Abstractions.Base;
            using RynorArch.Abstractions.Enums;

            [assembly: Architecture(
                Pattern = ArchitecturePattern.Cqrs,
                DbContextType = typeof(NotADbContext))]

            namespace Demo.Domain;

            [Entity]
            public partial class Trip : EntityBase
            {
                public string Destination { get; set; } = string.Empty;
            }

            public sealed class NotADbContext
            {
            }
            """);

        AssertContainsDiagnostic(result.Diagnostics, "RYNOR008", DiagnosticSeverity.Error);
    }

    [Fact]
    public void Reports_warning_when_caching_is_enabled_without_cqrs_pattern()
    {
        var result = GeneratorTestHarness.Run("""
            using RynorArch.Abstractions.Attributes;
            using RynorArch.Abstractions.Base;
            using RynorArch.Abstractions.Enums;
            using Microsoft.EntityFrameworkCore;

            [assembly: Architecture(
                Pattern = ArchitecturePattern.Repository,
                GenerateCachingDecorators = true)]

            namespace Demo.Domain;

            [Entity]
            public partial class Trip : EntityBase
            {
                public string Destination { get; set; } = string.Empty;
            }

            public sealed class AppDbContext : DbContext
            {
                public DbSet<Trip> Trips => Set<Trip>();
            }
            """);

        AssertContainsDiagnostic(result.Diagnostics, "RYNOR011", DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Reports_warning_when_endpoints_are_enabled_without_experimental_opt_in()
    {
        var result = GeneratorTestHarness.Run("""
            using RynorArch.Abstractions.Attributes;
            using RynorArch.Abstractions.Base;
            using RynorArch.Abstractions.Enums;
            using Microsoft.EntityFrameworkCore;

            [assembly: Architecture(
                Pattern = ArchitecturePattern.Cqrs,
                GenerateEndpoints = true)]

            namespace Demo.Domain;

            [Entity]
            public partial class Trip : EntityBase
            {
                public string Destination { get; set; } = string.Empty;
            }

            public sealed class AppDbContext : DbContext
            {
                public DbSet<Trip> Trips => Set<Trip>();
            }
            """);

        AssertContainsDiagnostic(result.Diagnostics, "RYNOR012", DiagnosticSeverity.Warning);
    }

    private static void AssertContainsDiagnostic(
        IEnumerable<Diagnostic> diagnostics,
        string id,
        DiagnosticSeverity severity)
    {
        Assert.Contains(
            diagnostics,
            diagnostic => diagnostic.Id == id && diagnostic.Severity == severity);
    }
}
