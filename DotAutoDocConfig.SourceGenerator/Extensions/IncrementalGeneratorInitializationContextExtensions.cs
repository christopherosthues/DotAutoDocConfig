using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotAutoDocConfig.SourceGenerator.Extensions;

internal static class IncrementalGeneratorInitializationContextExtensions
{
    extension(IncrementalGeneratorInitializationContext context)
    {
        public IncrementalValueProvider<(string projectDirectory, string projectName)> ProjectDirectoryAndNameProvider()
        {
            IncrementalValueProvider<(string projectDirectory, string projectName)> buildProps = context.AnalyzerConfigOptionsProvider
                .Select(static (optsProvider, _) =>
                {
                    AnalyzerConfigOptions opts = optsProvider.GlobalOptions;
                    if (!opts.TryGetValue("build_property.MSBuildProjectDirectory", out string? projectDirectory))
                    {
                        opts.TryGetValue("build_property.ProjectDir", out projectDirectory);
                    }

                    opts.TryGetValue("build_property.MSBuildProjectName", out string? projectName);
                    return (projectDirectory: projectDirectory ?? string.Empty, projectName: projectName ?? string.Empty);
                });
            return buildProps;
        }
    }
}
