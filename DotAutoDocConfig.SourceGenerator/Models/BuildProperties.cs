namespace DotAutoDocConfig.SourceGenerator.Models;

internal class BuildProperties(string projectDirectory, string projectName, bool isBuild)
{
    public string ProjectDirectory { get; } = projectDirectory;
    public string ProjectName { get; } = projectName;
    public bool IsBuild { get; } = isBuild;
}
