namespace DotAutoDocConfig.SourceGenerator.Models;

internal class BuildProperties(string projectDirectory)
{
    public string ProjectDirectory { get; } = projectDirectory;
}
