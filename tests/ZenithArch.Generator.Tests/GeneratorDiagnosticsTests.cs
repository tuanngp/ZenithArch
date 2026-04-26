using Microsoft.CodeAnalysis;
using Xunit;

namespace ZenithArch.Generator.Tests;

public sealed class GeneratorDiagnosticsTests
{
    [Fact]
    public void Reports_warning_when_no_entities_are_declared()
    {
        var result = GeneratorTestHarness.Run("""
            using ZenithArch.Abstractions.Attributes;
            using ZenithArch.Abstractions.Enums;

            [assembly: Architecture(Pattern = ArchitecturePattern.Cqrs)]

            namespace Demo;

            public sealed class Placeholder;
            """);

        AssertContainsDiagnostic(result.Diagnostics, "ZENITH001", DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Reports_error_when_aggregate_root_is_missing_entity_attribute()
    {
        var result = GeneratorTestHarness.Run("""
            using ZenithArch.Abstractions.Attributes;

            namespace Demo.Domain;

            [AggregateRoot]
            public sealed partial class Trip
            {
            }
            """);

        AssertContainsDiagnostic(result.Diagnostics, "ZENITH002", DiagnosticSeverity.Error);
    }

    [Fact]
    public void Reports_error_when_entity_is_not_partial()
    {
        var result = GeneratorTestHarness.Run("""
            using ZenithArch.Abstractions.Attributes;
            using ZenithArch.Abstractions.Base;
            using ZenithArch.Abstractions.Enums;

            [assembly: Architecture(Pattern = ArchitecturePattern.Repository)]

            namespace Demo.Domain;

            [Entity]
            public class Trip : EntityBase
            {
            }
            """);

        AssertContainsDiagnostic(result.Diagnostics, "ZENITH005", DiagnosticSeverity.Error);
    }

    [Fact]
    public void Reports_warning_for_unsupported_query_filter_type()
    {
        var result = GeneratorTestHarness.Run("""
            using ZenithArch.Abstractions.Attributes;
            using ZenithArch.Abstractions.Base;
            using ZenithArch.Abstractions.Enums;

            [assembly: Architecture(Pattern = ArchitecturePattern.Repository, UseSpecification = true)]

            namespace Demo.Domain;

            [Entity]
            public partial class Trip : EntityBase
            {
                [QueryFilter]
                public object? Metadata { get; init; }
            }
            """);

        AssertContainsDiagnostic(result.Diagnostics, "ZENITH004", DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Reports_error_when_architecture_configuration_is_missing()
    {
        var result = GeneratorTestHarness.Run("""
            using ZenithArch.Abstractions.Attributes;
            using ZenithArch.Abstractions.Base;

            namespace Demo.Domain;

            [Entity]
            public partial class Trip : EntityBase
            {
            }
            """);

        AssertContainsDiagnostic(result.Diagnostics, "ZENITH006", DiagnosticSeverity.Error);
    }

    [Fact]
    public void Reports_error_when_configured_dbcontext_type_is_not_derived_from_dbcontext()
    {
        var result = GeneratorTestHarness.Run("""
            using ZenithArch.Abstractions.Attributes;
            using ZenithArch.Abstractions.Base;
            using ZenithArch.Abstractions.Enums;

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

        AssertContainsDiagnostic(result.Diagnostics, "ZENITH008", DiagnosticSeverity.Error);
    }

    [Fact]
    public void Reports_warning_when_caching_is_enabled_without_cqrs_pattern()
    {
        var result = GeneratorTestHarness.Run("""
            using ZenithArch.Abstractions.Attributes;
            using ZenithArch.Abstractions.Base;
            using ZenithArch.Abstractions.Enums;
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

        AssertContainsDiagnostic(result.Diagnostics, "ZENITH011", DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Reports_warning_when_endpoints_are_enabled_without_experimental_opt_in()
    {
        var result = GeneratorTestHarness.Run("""
            using ZenithArch.Abstractions.Attributes;
            using ZenithArch.Abstractions.Base;
            using ZenithArch.Abstractions.Enums;
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

        AssertContainsDiagnostic(result.Diagnostics, "ZENITH012", DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Reports_warning_when_endpoint_hardening_mode_is_enabled_without_endpoint_generation()
    {
        var result = GeneratorTestHarness.Run("""
            using ZenithArch.Abstractions.Attributes;
            using ZenithArch.Abstractions.Base;
            using ZenithArch.Abstractions.Enums;
            using Microsoft.EntityFrameworkCore;

            [assembly: Architecture(
                Pattern = ArchitecturePattern.Cqrs,
                EndpointHardeningMode = EndpointHardeningMode.RequireAuthorization)]

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

        AssertContainsDiagnostic(result.Diagnostics, "ZENITH017", DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Reports_warning_when_endpoint_hardening_mode_is_enabled_without_experimental_opt_in()
    {
        var result = GeneratorTestHarness.Run("""
            using ZenithArch.Abstractions.Attributes;
            using ZenithArch.Abstractions.Base;
            using ZenithArch.Abstractions.Enums;
            using Microsoft.EntityFrameworkCore;

            [assembly: Architecture(
                Pattern = ArchitecturePattern.Cqrs,
                GenerateEndpoints = true,
                EndpointHardeningMode = EndpointHardeningMode.RequireAuthorization)]

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

        AssertContainsDiagnostic(result.Diagnostics, "ZENITH012", DiagnosticSeverity.Warning);
        AssertContainsDiagnostic(result.Diagnostics, "ZENITH017", DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Reports_warning_when_per_request_save_mode_is_enabled_without_generated_di()
    {
        var result = GeneratorTestHarness.Run("""
            using ZenithArch.Abstractions.Attributes;
            using ZenithArch.Abstractions.Base;
            using ZenithArch.Abstractions.Enums;
            using Microsoft.EntityFrameworkCore;

            [assembly: Architecture(
                Pattern = ArchitecturePattern.Cqrs,
                CqrsSaveMode = CqrsSaveMode.PerRequestTransaction)]

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

        AssertContainsDiagnostic(result.Diagnostics, "ZENITH013", DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Reports_info_for_legacy_explicit_configuration_that_can_use_profile_defaults()
    {
        var result = GeneratorTestHarness.Run("""
            using ZenithArch.Abstractions.Attributes;
            using ZenithArch.Abstractions.Base;
            using ZenithArch.Abstractions.Enums;
            using Microsoft.EntityFrameworkCore;

            [assembly: Architecture(
                Pattern = ArchitecturePattern.FullStack,
                UseSpecification = true,
                UseUnitOfWork = true,
                EnableValidation = true,
                GenerateDependencyInjection = true,
                GenerateDtos = true,
                GenerateEfConfigurations = true,
                GeneratePagination = true,
                CqrsSaveMode = CqrsSaveMode.PerRequestTransaction)]

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

        AssertContainsDiagnostic(result.Diagnostics, "ZENITH014", DiagnosticSeverity.Info);
    }

    [Fact]
    public void Reports_warning_when_validation_is_enabled_without_generated_di()
    {
        var result = GeneratorTestHarness.Run("""
            using ZenithArch.Abstractions.Attributes;
            using ZenithArch.Abstractions.Base;
            using ZenithArch.Abstractions.Enums;
            using Microsoft.EntityFrameworkCore;

            [assembly: Architecture(
                Pattern = ArchitecturePattern.Cqrs,
                EnableValidation = true,
                GenerateDependencyInjection = false)]

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

        AssertContainsDiagnostic(result.Diagnostics, "ZENITH015", DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Reports_info_when_endpoints_are_enabled_with_experimental_opt_in()
    {
        var result = GeneratorTestHarness.Run("""
            using ZenithArch.Abstractions.Attributes;
            using ZenithArch.Abstractions.Base;
            using ZenithArch.Abstractions.Enums;
            using Microsoft.EntityFrameworkCore;

            [assembly: Architecture(
                Pattern = ArchitecturePattern.Cqrs,
                GenerateEndpoints = true,
                EnableExperimentalEndpoints = true)]

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

        AssertContainsDiagnostic(result.Diagnostics, "ZENITH016", DiagnosticSeverity.Info);
    }

    [Fact]
    public void Does_not_report_minimal_endpoint_behavior_notice_when_hardening_mode_is_enabled()
    {
        var result = GeneratorTestHarness.Run("""
            using ZenithArch.Abstractions.Attributes;
            using ZenithArch.Abstractions.Base;
            using ZenithArch.Abstractions.Enums;
            using Microsoft.EntityFrameworkCore;

            [assembly: Architecture(
                Pattern = ArchitecturePattern.Cqrs,
                GenerateEndpoints = true,
                EnableExperimentalEndpoints = true,
                EndpointHardeningMode = EndpointHardeningMode.RequireAuthorization)]

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

        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Id == "ZENITH009");
        AssertContainsDiagnostic(result.Diagnostics, "ZENITH016", DiagnosticSeverity.Info);
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
