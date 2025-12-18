using System;
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
                    if (!opts.TryGetValue("build_property.MSBuildProjectDirectory", out string? projectDirectory))
                    {
                        opts.TryGetValue("build_property.ProjectDir", out projectDirectory);
                    }

                    opts.TryGetValue("build_property.MSBuildProjectName", out string? projectName);

                    bool isBuild = !opts.TryGetValue("build_property.DesignTimeBuild", out string? designTime) &&
                                   string.Equals(designTime, "true", StringComparison.OrdinalIgnoreCase); // TODO: Check logic
                    isBuild = true;
                    return new BuildProperties(projectDirectory ?? string.Empty, projectName ?? string.Empty, isBuild);
                });
            return buildProps;
        }
    }
}
