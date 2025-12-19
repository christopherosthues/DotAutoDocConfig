using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.Extensions;

internal static class SourceProductionContextExtensions
{
    extension(SourceProductionContext context)
    {
        public void LogInfo(string message, params object?[]? args)
        {
            // Emit an informational diagnostic that shows up in the build output
            DiagnosticDescriptor descriptor = new(
                id: DiagnosticIds.InfoType,
                title: "DocumentationGeneratorInfo",
                messageFormat: message,
                category: "DocumentationGenerator",
                defaultSeverity: DiagnosticSeverity.Info,
                isEnabledByDefault: true);
            Diagnostic diagnostic = Diagnostic.Create(descriptor, Location.None, args);
            context.ReportDiagnostic(diagnostic);
        }

        public void LogWarning(string message, params object?[]? args)
        {
            // Emit a warning diagnostic that shows up in the build output
            DiagnosticDescriptor descriptor = new(
                id: DiagnosticIds.OutputDirectoryNotADirectory,
                title: "DocumentationGeneratorWarning",
                messageFormat: message,
                category: "DocumentationGenerator",
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true);
            Diagnostic diagnostic = Diagnostic.Create(descriptor, Location.None, args);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
