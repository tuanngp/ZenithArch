using Microsoft.CodeAnalysis;

namespace RynorArch.Generator.Helpers;

internal static class DiagnosticReporter
{
    public static void Report(
        SourceProductionContext context,
        DiagnosticDescriptor descriptor,
        Location location)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location));

    public static void Report(
        SourceProductionContext context,
        DiagnosticDescriptor descriptor,
        Location location,
        params object?[]? messageArgs)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, messageArgs));
}