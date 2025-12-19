namespace DotAutoDocConfig.SourceGenerator.Models;

internal class BuildProperties(string projectDirectory, string projectName)
{
    public string ProjectDirectory { get; } = projectDirectory;
    public string ProjectName { get; } = projectName;
}
