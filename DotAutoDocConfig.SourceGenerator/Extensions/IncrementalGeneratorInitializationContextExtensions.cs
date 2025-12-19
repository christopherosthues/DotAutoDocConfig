using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotAutoDocConfig.SourceGenerator.Extensions;

internal static class IncrementalGeneratorInitializationContextExtensions
{
    extension(IncrementalGeneratorInitializationContext context)
    {
        public IncrementalValueProvider<BuildProperties> BuildPropsProvider()
        {
            IncrementalValueProvider<BuildProperties> buildProps = context.AnalyzerConfigOptionsProvider
                .Select(static (optsProvider, _) =>
                {
                    AnalyzerConfigOptions opts = optsProvider.GlobalOptions;
                    if (!opts.TryGetValue("build_property.MSBuildProjectDirectory", out string? projectDirectory) &&
                        !opts.TryGetValue("build_property.ProjectDir", out projectDirectory) &&
                        !opts.TryGetValue("build_property.projectdir", out projectDirectory))
                    {
                        projectDirectory = string.Empty;
                    }

                    if (!opts.TryGetValue("build_property.MSBuildProjectName", out string? projectName) &&
                        !opts.TryGetValue("build_property.rootnamespace", out projectName))
                    {
                        projectName = string.Empty;
                    }

                    return new BuildProperties(projectDirectory, projectName);
                });
            return buildProps;
        }
    }
}
