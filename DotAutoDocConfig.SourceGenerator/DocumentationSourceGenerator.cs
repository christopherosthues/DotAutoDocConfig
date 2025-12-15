using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.IO;
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace DotAutoDocConfig.SourceGenerator;

/// <summary>
/// A sample source generator that creates a custom report based on class properties. The target class should be annotated with the 'Generators.ReportAttribute' attribute.
/// When using the source code as a baseline, an incremental source generator is preferable because it reduces the performance overhead.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class DocumentationSourceGenerator : IIncrementalGenerator
{
    public enum LocalFormat : byte
    {
        None = 0,
        AsciiDoc = 1,
        Markdown = 2,
        Html = 3
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter classes annotated with the [Documentation] attribute. Only filtered Syntax Nodes can trigger code generation.
        IncrementalValuesProvider<ClassDeclarationSyntax> provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "DotAutoDocConfig.Core.ComponentModel.Attributes.DocumentationAttribute",
                static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, _) => (ClassDeclarationSyntax)ctx.TargetNode);

        // Try to read the project directory and project name from analyzer config global options (MSBuildProjectDirectory/MSBuildProjectName).
        string projectDirectory = string.Empty;
        string projectName = string.Empty;
        var globalOptions = context.AnalyzerConfigOptionsProvider.GlobalOptions;
        if (!globalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out projectDirectory))
        {
            globalOptions.TryGetValue("build_property.ProjectDir", out projectDirectory);
        }
        globalOptions.TryGetValue("build_property.MSBuildProjectName", out projectName);

        // Generate the source code; pass projectDirectory and projectName so we can resolve relative paths against the project/repo root and write to obj/DocsGenerated.
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
            ((ctx, t) => GenerateCode(ctx, t.Left, t.Right, projectDirectory, projectName)));
    }

    // Helper: resolve repository/project root by walking up from a file path looking for .git, *.sln or Directory.Build.props
#pragma warning disable RS1035 // Do not do file IO in analyzers
    private static string ResolveRepositoryRoot(string? startFilePath)
    {
        try
        {
            if (string.IsNullOrEmpty(startFilePath))
                return Directory.GetCurrentDirectory();

            string? current = Path.GetDirectoryName(startFilePath);
            while (!string.IsNullOrEmpty(current))
            {
                // check .git
                if (Directory.Exists(Path.Combine(current, ".git")))
                    return current;
                // check solution files
                string[] slnFiles = Directory.GetFiles(current, "*.sln");
                if (slnFiles.Length > 0)
                    return current;
                // check for slnx or directory build props
                if (File.Exists(Path.Combine(current, "DotAutoDocConfig.slnx")) || File.Exists(Path.Combine(current, "Directory.Build.props")))
                    return current;

                string parent = Path.GetDirectoryName(current);
                if (string.IsNullOrEmpty(parent) || parent == current)
                    break;
                current = parent;
            }

            return Directory.GetCurrentDirectory();
        }
        catch
        {
            return Directory.GetCurrentDirectory();
        }
    }
#pragma warning restore RS1035

    private static (INamedTypeSymbol?, List<DocumentationOptionsDataModel>) GetDocumentationDataModels(
        Compilation compilation,
        ClassDeclarationSyntax classDeclarationSyntax)
    {
        List<DocumentationOptionsDataModel> documentationDataModels = new();

        // We need to get semantic model of the class to retrieve
        SemanticModel semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
        if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
            return (null, documentationDataModels);
        // Go through all attributes of the class.
        foreach (AttributeData attributeData in classSymbol.GetAttributes())
        {
            string attributeName = attributeData.AttributeClass?.ToDisplayString() ?? string.Empty;
            // Check the full name of the [Documentation] attribute.
            if (attributeName != "DotAutoDocConfig.Core.ComponentModel.Attributes.DocumentationAttribute")
                continue;
            // Retrieve constructor arguments.
            if (attributeData.ConstructorArguments.Length != 2)
                continue;
            // Read numeric value of the enum (as byte/int) to avoid referencing the external enum type.
            byte formatValue = 0;
            object? raw = attributeData.ConstructorArguments[0].Value;
            if (raw is int intVal)
                formatValue = (byte)intVal;
            else if (raw is byte bVal)
                formatValue = bVal;

            string outputPath = attributeData.ConstructorArguments[1].Value!.ToString() ?? string.Empty;
            documentationDataModels.Add(new DocumentationOptionsDataModel
            {
                Format = formatValue,
                OutputPath = outputPath
            });
        }
        return (classSymbol, documentationDataModels);
    }

    /// <summary>
    /// Generate code action.
    /// It will be executed on specific nodes (ClassDeclarationSyntax annotated with the [Report] attribute) changed by the user.
    /// </summary>
    /// <param name="context">Source generation context used to add source files.</param>
    /// <param name="compilation">Compilation used to provide access to the Semantic Model.</param>
    /// <param name="classDeclarations">Nodes annotated with the [Report] attribute that trigger the generate action.</param>
    private void GenerateCode(SourceProductionContext context, Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classDeclarations, string projectDirectory, string projectName)
    {
        // Go through all filtered class declarations.
        foreach (ClassDeclarationSyntax? classDeclarationSyntax in classDeclarations)
        {
            // We need to get semantic model of the class to retrieve metadata.
            SemanticModel semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

            // Symbols allow us to get the compile-time information.
            if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
                continue;

            // determine repository root: prefer projectDirectory from analyzer config; otherwise fallback to the directory of the source file.
            string repoRoot = projectDirectory;
            if (string.IsNullOrEmpty(repoRoot))
            {
                string? filePath = classDeclarationSyntax.SyntaxTree.FilePath;
                repoRoot = Path.GetDirectoryName(filePath) ?? string.Empty;
            }

            // Get per-attribute documentation options
            (INamedTypeSymbol? symbol, List<DocumentationOptionsDataModel> docs) = GetDocumentationDataModels(compilation, classDeclarationSyntax);
            if (symbol is null)
                continue;

            foreach (DocumentationOptionsDataModel? docOptions in docs)
            {
                // Collect flattened entries recursively
                List<DocumentationDataModel> entries = GeneratorHelpers.CollectDocumentationEntries(classSymbol, compilation);

                // Build content
                StringBuilder sb = new();
                LocalFormat fmt = (LocalFormat)docOptions.Format;
                switch (fmt)
                {
                    case LocalFormat.AsciiDoc:
                        DocumentationGenerators.AsciiDocGenerator.GenerateAsciiDoc(sb, classSymbol, entries);
                        break;
                    case LocalFormat.Markdown:
                        DocumentationGenerators.MarkdownGenerator.Generate(sb, classSymbol, entries);
                        break;
                    default:
                        DocumentationGenerators.MarkdownGenerator.Generate(sb, classSymbol, entries);
                        break;
                }

                // Write the documentation to the output path specified in the attribute.
                // If the path is relative, resolve it against the repository/project root and write into obj/DocsGenerated/$(projectName)/<requestedPath>.
                // MSBuild target will later copy from obj/DocsGenerated to the final project location.
                string requestedPath = docOptions.OutputPath ?? string.Empty;
#pragma warning disable RS1035 // Do not do file IO in analyzers
                try
                {
                    if (Path.IsPathRooted(requestedPath))
                    {
                        // absolute path requested -> write directly there
                        string fullPath = requestedPath;
                        string? dir = Path.GetDirectoryName(fullPath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        File.WriteAllText(fullPath, sb.ToString(), Encoding.UTF8);
                    }
                    else
                    {
                        string baseIntermediate = Path.GetFullPath(Path.Combine(repoRoot, "obj", "DocsGenerated", string.IsNullOrEmpty(projectName) ? "DefaultProject" : projectName));
                        string fullIntermediatePath = Path.GetFullPath(Path.Combine(baseIntermediate, requestedPath));

                        string? dir = Path.GetDirectoryName(fullIntermediatePath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        File.WriteAllText(fullIntermediatePath, sb.ToString(), Encoding.UTF8);
                    }
                }
                catch (System.Exception ex)
                {
                    // If writing fails, emit a diagnostic but do not throw.
                    DiagnosticDescriptor desc = new("DDG001", "DocumentationGeneratorWriteFailed", "Failed to write documentation file '{0}': {1}", "DocumentationGenerator", DiagnosticSeverity.Warning, true);
                    Diagnostic diagnostic = Diagnostic.Create(desc, Location.None, requestedPath, ex.Message);
                    context.ReportDiagnostic(diagnostic);
                }
#pragma warning restore RS1035
            }
        }
    }
}
